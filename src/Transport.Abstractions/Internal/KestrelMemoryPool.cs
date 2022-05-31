// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal
{
    public static class KestrelMemoryPool
    {
        public static MemoryPool<byte> Create()
        {
#if DEBUG
            return new DiagnosticMemoryPool(CreateSlabMemoryPool());
#else
            return CreateSlabMemoryPool();
#endif
        }

        public static MemoryPool<byte> CreateSlabMemoryPool()
        {
            return new SlabMemoryPool();
        }

        public static readonly int MinimumSegmentSize = 4096;
    }

    /// <summary>
    /// Used to allocate and distribute re-usable blocks of memory.
    /// </summary>
    internal class SlabMemoryPool : MemoryPool<byte>
    {
        /// <summary>
        /// The size of a block. 4096 is chosen because most operating systems use 4k pages.
        /// </summary>
        private const int _blockSize = 4096;

        /// <summary>
        /// Allocating 32 contiguous blocks per slab makes the slab size 128k. This is larger than the 85k size which will place the memory
        /// in the large object heap. This means the GC will not try to relocate this array, so the fact it remains pinned does not negatively
        /// affect memory management's compactification.
        /// </summary>
        private const int _blockCount = 32;

        /// <summary>
        /// Max allocation block size for pooled blocks,
        /// larger values can be leased but they will be disposed after use rather than returned to the pool.
        /// </summary>
        public override int MaxBufferSize { get; } = _blockSize;

        /// <summary>
        /// 4096 * 32 gives you a slabLength of 128k contiguous bytes allocated per slab
        /// </summary>
        private static readonly int _slabLength = _blockSize * _blockCount;

        /// <summary>
        /// Thread-safe collection of blocks which are currently in the pool. A slab will pre-allocate all of the block tracking objects
        /// and add them to this collection. When memory is requested it is taken from here first, and when it is returned it is re-added.
        /// </summary>
        private readonly ConcurrentQueue<MemoryPoolBlock> _blocks = new ConcurrentQueue<MemoryPoolBlock>();

        /// <summary>
        /// Thread-safe collection of slabs which have been allocated by this pool. As long as a slab is in this collection and slab.IsActive,
        /// the blocks will be added to _blocks when returned.
        /// </summary>
        private readonly ConcurrentStack<MemoryPoolSlab> _slabs = new ConcurrentStack<MemoryPoolSlab>();

        /// <summary>
        /// This is part of implementing the IDisposable pattern.
        /// </summary>
        private bool _isDisposed; // To detect redundant calls

        private int _totalAllocatedBlocks;

        private readonly object _disposeSync = new object();

        /// <summary>
        /// This default value passed in to Rent to use the default value for the pool.
        /// </summary>
        private const int AnySize = -1;

        public override IMemoryOwner<byte> Rent(int size = AnySize)
        {
            if (size > _blockSize)
            {
                MemoryPoolThrowHelper.ThrowArgumentOutOfRangeException_BufferRequestTooLarge(_blockSize);
            }

            var block = Lease();
            return block;
        }

        /// <summary>
        /// Called to take a block from the pool.
        /// </summary>
        /// <returns>The block that is reserved for the called. It must be passed to Return when it is no longer being used.</returns>
        private MemoryPoolBlock Lease()
        {
            if (_isDisposed)
            {
                MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryPool);
            }

            if (_blocks.TryDequeue(out MemoryPoolBlock block))
            {
                // block successfully taken from the stack - return it

                block.Lease();
                return block;
            }
            // no blocks available - grow the pool
            block = AllocateSlab();
            block.Lease();
            return block;
        }

        /// <summary>
        /// Internal method called when a block is requested and the pool is empty. It allocates one additional slab, creates all of the
        /// block tracking objects, and adds them all to the pool.
        /// </summary>
        private MemoryPoolBlock AllocateSlab()
        {
            var slab = MemoryPoolSlab.Create(_slabLength);
            _slabs.Push(slab);

            var basePtr = slab.NativePointer;
            // Page align the blocks
            var offset = (int)((((ulong)basePtr + (uint)_blockSize - 1) & ~((uint)_blockSize - 1)) - (ulong)basePtr);
            // Ensure page aligned
            Debug.Assert(((ulong)basePtr + (uint)offset) % _blockSize == 0);

            var blockCount = (_slabLength - offset) / _blockSize;
            Interlocked.Add(ref _totalAllocatedBlocks, blockCount);

            MemoryPoolBlock block = null;

            for (int i = 0; i < blockCount; i++)
            {
                block = new MemoryPoolBlock(this, slab, offset, _blockSize);

                if (i != blockCount - 1) // last block
                {
#if BLOCK_LEASE_TRACKING
                    block.IsLeased = true;
#endif
                    Return(block);
                }

                offset += _blockSize;
            }

            return block;
        }

        /// <summary>
        /// Called to return a block to the pool. Once Return has been called the memory no longer belongs to the caller, and
        /// Very Bad Things will happen if the memory is read of modified subsequently. If a caller fails to call Return and the
        /// block tracking object is garbage collected, the block tracking object's finalizer will automatically re-create and return
        /// a new tracking object into the pool. This will only happen if there is a bug in the server, however it is necessary to avoid
        /// leaving "dead zones" in the slab due to lost block tracking objects.
        /// </summary>
        /// <param name="block">The block to return. It must have been acquired by calling Lease on the same memory pool instance.</param>
        internal void Return(MemoryPoolBlock block)
        {
#if BLOCK_LEASE_TRACKING
            Debug.Assert(block.Pool == this, "Returned block was not leased from this pool");
            Debug.Assert(block.IsLeased, $"Block being returned to pool twice: {block.Leaser}{Environment.NewLine}");
            block.IsLeased = false;
#endif

            if (!_isDisposed)
            {
                _blocks.Enqueue(block);
            }
            else
            {
                GC.SuppressFinalize(block);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (_disposeSync)
            {
                _isDisposed = true;

                if (disposing)
                {
                    while (_slabs.TryPop(out MemoryPoolSlab slab))
                    {
                        // dispose managed state (managed objects).
                        slab.Dispose();
                    }
                }

                // Discard blocks in pool
                while (_blocks.TryDequeue(out MemoryPoolBlock block))
                {
                    GC.SuppressFinalize(block);
                }
            }
        }
    }

    /// <summary>
    /// Block tracking object used by the byte buffer memory pool. A slab is a large allocation which is divided into smaller blocks. The
    /// individual blocks are then treated as independent array segments.
    /// </summary>
    internal sealed class MemoryPoolBlock : IMemoryOwner<byte>
    {
        private readonly int _offset;
        private readonly int _length;

        /// <summary>
        /// This object cannot be instantiated outside of the static Create method
        /// </summary>
        internal MemoryPoolBlock(SlabMemoryPool pool, MemoryPoolSlab slab, int offset, int length)
        {
            _offset = offset;
            _length = length;

            Pool = pool;
            Slab = slab;

            Memory = MemoryMarshal.CreateFromPinnedArray(slab.Array, _offset, _length);
        }

        /// <summary>
        /// Back-reference to the memory pool which this block was allocated from. It may only be returned to this pool.
        /// </summary>
        public SlabMemoryPool Pool { get; }

        /// <summary>
        /// Back-reference to the slab from which this block was taken, or null if it is one-time-use memory.
        /// </summary>
        public MemoryPoolSlab Slab { get; }

        public Memory<byte> Memory { get; }

        ~MemoryPoolBlock()
        {
            if (Slab != null && Slab.IsActive)
            {
                // Need to make a new object because this one is being finalized
                Pool.Return(new MemoryPoolBlock(Pool, Slab, _offset, _length));
            }
        }

        public void Dispose()
        {
            Pool.Return(this);
        }

        public void Lease()
        {
        }
    }

    /// <summary>
    /// Slab tracking object used by the byte buffer memory pool. A slab is a large allocation which is divided into smaller blocks. The
    /// individual blocks are then treated as independent array segments.
    /// </summary>
    internal class MemoryPoolSlab : IDisposable
    {
        /// <summary>
        /// This handle pins the managed array in memory until the slab is disposed. This prevents it from being
        /// relocated and enables any subsections of the array to be used as native memory pointers to P/Invoked API calls.
        /// </summary>
        private GCHandle _gcHandle;
        private bool _isDisposed;

        public MemoryPoolSlab(byte[] data)
        {
            Array = data;
            _gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            NativePointer = _gcHandle.AddrOfPinnedObject();
        }

        /// <summary>
        /// True as long as the blocks from this slab are to be considered returnable to the pool. In order to shrink the
        /// memory pool size an entire slab must be removed. That is done by (1) setting IsActive to false and removing the
        /// slab from the pool's _slabs collection, (2) as each block currently in use is Return()ed to the pool it will
        /// be allowed to be garbage collected rather than re-pooled, and (3) when all block tracking objects are garbage
        /// collected and the slab is no longer references the slab will be garbage collected and the memory unpinned will
        /// be unpinned by the slab's Dispose.
        /// </summary>
        public bool IsActive => !_isDisposed;

        public IntPtr NativePointer { get; private set; }

        public byte[] Array { get; private set; }

        public static MemoryPoolSlab Create(int length)
        {
            // allocate and pin requested memory length
            var array = new byte[length];

            // allocate and return slab tracking object
            return new MemoryPoolSlab(array);
        }

        protected void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            Array = null;
            NativePointer = IntPtr.Zero; ;

            if (_gcHandle.IsAllocated)
            {
                _gcHandle.Free();
            }
        }

        ~MemoryPoolSlab()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    internal class MemoryPoolThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(int sourceLength, int offset)
        {
            throw GetArgumentOutOfRangeException(sourceLength, offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(int sourceLength, int offset)
        {
            if ((uint)offset > (uint)sourceLength)
            {
                // Offset is negative or less than array length
                return new ArgumentOutOfRangeException(GetArgumentName(ExceptionArgument.offset));
            }

            // The third parameter (not passed) length must be out of range
            return new ArgumentOutOfRangeException(GetArgumentName(ExceptionArgument.length));
        }

        public static void ThrowInvalidOperationException_PinCountZero(DiagnosticPoolBlock block)
        {
            throw new InvalidOperationException(GenerateMessage("Can't unpin, pin count is zero", block));
        }

        public static void ThrowInvalidOperationException_ReturningPinnedBlock(DiagnosticPoolBlock block)
        {
            throw new InvalidOperationException(GenerateMessage("Disposing pinned block", block));
        }

        public static void ThrowInvalidOperationException_DoubleDispose()
        {
            throw new InvalidOperationException("Object is being disposed twice");
        }

        public static void ThrowInvalidOperationException_BlockDoubleDispose(DiagnosticPoolBlock block)
        {
            throw new InvalidOperationException("Block is being disposed twice");
        }

        public static void ThrowInvalidOperationException_BlockReturnedToDisposedPool(DiagnosticPoolBlock block)
        {
            throw new InvalidOperationException(GenerateMessage("Block is being returned to disposed pool", block));
        }

        public static void ThrowInvalidOperationException_BlockIsBackedByDisposedSlab(DiagnosticPoolBlock block)
        {
            throw new InvalidOperationException(GenerateMessage("Block is backed by disposed slab", block));
        }

        public static void ThrowInvalidOperationException_DisposingPoolWithActiveBlocks(int returned, int total, DiagnosticPoolBlock[] blocks)
        {
            throw new InvalidOperationException(GenerateMessage($"Memory pool with active blocks is being disposed, {returned} of {total} returned", blocks));
        }

        public static void ThrowInvalidOperationException_BlocksWereNotReturnedInTime(int returned, int total, DiagnosticPoolBlock[] blocks)
        {
            throw new InvalidOperationException(GenerateMessage($"Blocks were not returned in time, {returned} of {total} returned ", blocks));
        }

        private static string GenerateMessage(string message, params DiagnosticPoolBlock[] blocks)
        {
            StringBuilder builder = new StringBuilder(message);
            foreach (var diagnosticPoolBlock in blocks)
            {
                if (diagnosticPoolBlock.Leaser != null)
                {
                    builder.AppendLine();

                    builder.AppendLine("Block leased from:");
                    builder.AppendLine(diagnosticPoolBlock.Leaser.ToString());
                }
            }

            return builder.ToString();
        }

        public static void ThrowArgumentOutOfRangeException_BufferRequestTooLarge(int maxSize)
        {
            throw GetArgumentOutOfRangeException_BufferRequestTooLarge(maxSize);
        }

        public static void ThrowObjectDisposedException(ExceptionArgument argument)
        {
            throw GetObjectDisposedException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException_BufferRequestTooLarge(int maxSize)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(ExceptionArgument.size), $"Cannot allocate more than {maxSize} bytes in a single buffer");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectDisposedException GetObjectDisposedException(ExceptionArgument argument)
        {
            return new ObjectDisposedException(GetArgumentName(argument));
        }

        private static string GetArgumentName(ExceptionArgument argument)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument), "The enum value is not defined, please check the ExceptionArgument Enum.");

            return argument.ToString();
        }

        internal enum ExceptionArgument
        {
            size,
            offset,
            length,
            MemoryPoolBlock,
            MemoryPool
        }
    }
    /// <summary>
    /// Block tracking object used by the byte buffer memory pool. A slab is a large allocation which is divided into smaller blocks. The
    /// individual blocks are then treated as independent array segments.
    /// </summary>
    internal sealed class DiagnosticPoolBlock : MemoryManager<byte>
    {
        /// <summary>
        /// Back-reference to the memory pool which this block was allocated from. It may only be returned to this pool.
        /// </summary>
        private readonly DiagnosticMemoryPool _pool;

        private readonly IMemoryOwner<byte> _memoryOwner;
        private MemoryHandle? _memoryHandle;
        private Memory<byte> _memory;

        private readonly object _syncObj = new object();
        private bool _isDisposed;
        private int _pinCount;


        /// <summary>
        /// This object cannot be instantiated outside of the static Create method
        /// </summary>
        internal DiagnosticPoolBlock(DiagnosticMemoryPool pool, IMemoryOwner<byte> memoryOwner)
        {
            _pool = pool;
            _memoryOwner = memoryOwner;
            _memory = memoryOwner.Memory;
        }

        public override Memory<byte> Memory
        {
            get
            {
                try
                {
                    lock (_syncObj)
                    {
                        if (_isDisposed)
                        {
                            MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryPoolBlock);
                        }

                        if (_pool.IsDisposed)
                        {
                            MemoryPoolThrowHelper.ThrowInvalidOperationException_BlockIsBackedByDisposedSlab(this);
                        }

                        return CreateMemory(_memory.Length);
                    }
                }
                catch (Exception exception)
                {
                    _pool.ReportException(exception);
                    throw;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                lock (_syncObj)
                {
                    if (Volatile.Read(ref _pinCount) > 0)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_ReturningPinnedBlock(this);
                    }

                    if (_isDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_BlockDoubleDispose(this);
                    }

                    _memoryOwner.Dispose();

                    _pool.Return(this);

                    _isDisposed = true;
                }
            }
            catch (Exception exception)
            {
                _pool.ReportException(exception);
                throw;
            }
        }

        public override Span<byte> GetSpan()
        {
            try
            {
                lock (_syncObj)
                {
                    if (_isDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryPoolBlock);
                    }

                    if (_pool.IsDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_BlockIsBackedByDisposedSlab(this);
                    }

                    return _memory.Span;
                }
            }
            catch (Exception exception)
            {
                _pool.ReportException(exception);
                throw;
            }
        }

        public override MemoryHandle Pin(int byteOffset = 0)
        {
            try
            {
                lock (_syncObj)
                {
                    if (_isDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryPoolBlock);
                    }

                    if (_pool.IsDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_BlockIsBackedByDisposedSlab(this);
                    }

                    if (byteOffset < 0 || byteOffset > _memory.Length)
                    {
                        MemoryPoolThrowHelper.ThrowArgumentOutOfRangeException(_memory.Length, byteOffset);
                    }

                    _pinCount++;

                    _memoryHandle = _memoryHandle ?? _memory.Pin();

                    unsafe
                    {
                        return new MemoryHandle(((IntPtr)_memoryHandle.Value.Pointer + byteOffset).ToPointer(), default, this);
                    }
                }
            }
            catch (Exception exception)
            {
                _pool.ReportException(exception);
                throw;
            }
        }

        protected override bool TryGetArray(out ArraySegment<byte> segment)
        {
            try
            {
                lock (_syncObj)
                {
                    if (_isDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryPoolBlock);
                    }

                    if (_pool.IsDisposed)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_BlockIsBackedByDisposedSlab(this);
                    }

                    return MemoryMarshal.TryGetArray(_memory, out segment);
                }
            }
            catch (Exception exception)
            {
                _pool.ReportException(exception);
                throw;
            }
        }

        public override void Unpin()
        {
            try
            {
                lock (_syncObj)
                {
                    if (_pinCount == 0)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_PinCountZero(this);
                    }

                    _pinCount--;

                    if (_pinCount == 0)
                    {
                        Debug.Assert(_memoryHandle.HasValue);
                        _memoryHandle.Value.Dispose();
                        _memoryHandle = null;
                    }
                }
            }
            catch (Exception exception)
            {
                _pool.ReportException(exception);
                throw;
            }
        }

        public StackTrace Leaser { get; set; }

        public void Track()
        {
            Leaser = new StackTrace(false);
        }
    }

    /// <summary>
    /// Used to allocate and distribute re-usable blocks of memory.
    /// </summary>
    internal class DiagnosticMemoryPool : MemoryPool<byte>
    {
        private readonly MemoryPool<byte> _pool;

        private readonly bool _allowLateReturn;

        private readonly bool _rentTracking;

        private readonly object _syncObj;

        private readonly HashSet<DiagnosticPoolBlock> _blocks;

        private readonly List<Exception> _blockAccessExceptions;

        private readonly TaskCompletionSource<object> _allBlocksReturned;

        private int _totalBlocks;

        /// <summary>
        /// This default value passed in to Rent to use the default value for the pool.
        /// </summary>
        private const int AnySize = -1;

        public DiagnosticMemoryPool(MemoryPool<byte> pool, bool allowLateReturn = false, bool rentTracking = false)
        {
            _pool = pool;
            _allowLateReturn = allowLateReturn;
            _rentTracking = rentTracking;
            _blocks = new HashSet<DiagnosticPoolBlock>();
            _syncObj = new object();
            _allBlocksReturned = new TaskCompletionSource<object>();
            _blockAccessExceptions = new List<Exception>();
        }

        public bool IsDisposed { get; private set; }

        public override IMemoryOwner<byte> Rent(int size = AnySize)
        {
            lock (_syncObj)
            {
                if (IsDisposed)
                {
                    MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryPool);
                }

                var diagnosticPoolBlock = new DiagnosticPoolBlock(this, _pool.Rent(size));
                if (_rentTracking)
                {
                    diagnosticPoolBlock.Track();
                }
                _totalBlocks++;
                _blocks.Add(diagnosticPoolBlock);
                return diagnosticPoolBlock;
            }
        }

        public override int MaxBufferSize => _pool.MaxBufferSize;

        internal void Return(DiagnosticPoolBlock block)
        {
            bool returnedAllBlocks;
            lock (_syncObj)
            {
                _blocks.Remove(block);
                returnedAllBlocks = _blocks.Count == 0;
            }

            if (IsDisposed)
            {
                if (!_allowLateReturn)
                {
                    MemoryPoolThrowHelper.ThrowInvalidOperationException_BlockReturnedToDisposedPool(block);
                }

                if (returnedAllBlocks)
                {
                    SetAllBlocksReturned();
                }
            }

        }

        internal void ReportException(Exception exception)
        {
            lock (_syncObj)
            {
                _blockAccessExceptions.Add(exception);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                MemoryPoolThrowHelper.ThrowInvalidOperationException_DoubleDispose();
            }

            bool allBlocksReturned = false;
            try
            {
                lock (_syncObj)
                {
                    IsDisposed = true;
                    allBlocksReturned = _blocks.Count == 0;
                    if (!allBlocksReturned && !_allowLateReturn)
                    {
                        MemoryPoolThrowHelper.ThrowInvalidOperationException_DisposingPoolWithActiveBlocks(_totalBlocks - _blocks.Count, _totalBlocks, _blocks.ToArray());
                    }

                    if (_blockAccessExceptions.Any())
                    {
                        throw CreateAccessExceptions();
                    }
                }
            }
            finally
            {
                if (allBlocksReturned)
                {
                    SetAllBlocksReturned();
                }
            }
        }

        private void SetAllBlocksReturned()
        {
            if (_blockAccessExceptions.Any())
            {
                _allBlocksReturned.SetException(CreateAccessExceptions());
            }
            else
            {
                _allBlocksReturned.SetResult(null);
            }
        }

        private AggregateException CreateAccessExceptions()
        {
            return new AggregateException("Exceptions occurred while accessing blocks", _blockAccessExceptions.ToArray());
        }

        public async Task WhenAllBlocksReturnedAsync(TimeSpan timeout)
        {
            var task = await Task.WhenAny(_allBlocksReturned.Task, Task.Delay(timeout));
            if (task != _allBlocksReturned.Task)
            {
                MemoryPoolThrowHelper.ThrowInvalidOperationException_BlocksWereNotReturnedInTime(_totalBlocks - _blocks.Count, _totalBlocks, _blocks.ToArray());
            }

            await task;
        }
    }
}
