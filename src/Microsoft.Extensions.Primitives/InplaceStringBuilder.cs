
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Primitives
{
    [DebuggerDisplay("Value = {_value}")]
    public struct InplaceStringBuilder
    {
        private int _offset;
        private int _capacity;
        private string _value;

        public InplaceStringBuilder(int capacity) : this()
        {
            if (capacity < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
            }

            _capacity = capacity;
        }

        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
                }

                // _offset > 0 indicates writing state
                if (_offset > 0)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Capacity_CannotChangeAfterWriteStarted);
                }

                _capacity = value;
            }
        }

        public void Append(string value)
        {
            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }

            Append(value, 0, value.Length);
        }

        public void Append(StringSegment segment)
        {
            Append(segment.Buffer, segment.Offset, segment.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Append(string value, int offset, int count)
        {
            EnsureValueIsInitialized();

            if (value == null
                || offset < 0
                || value.Length - offset < count
                || Capacity - _offset < count)
            {
                ThrowValidationError(value, offset, count);
            }

            fixed (char* destination = _value)
            fixed (char* source = value)
            {
                Unsafe.CopyBlockUnaligned(destination + _offset, source + offset, (uint)count * 2);
                _offset += count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Append(char c)
        {
            EnsureValueIsInitialized();

            if (_offset >= Capacity)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Capacity_NotEnough, 1, Capacity - _offset);
            }

            fixed (char* destination = _value)
            {
                destination[_offset++] = c;
            }
        }

        public override string ToString()
        {
            if (Capacity != _offset)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Capacity_NotUsedEntirely, Capacity, _offset);
            }

            return _value;
        }

        private void EnsureValueIsInitialized()
        {
            if (_value == null)
            {
                _value = new string('\0', _capacity);
            }
        }

        private void ThrowValidationError(string value, int offset, int count)
        {
            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }

            if (offset < 0 || value.Length - offset < count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.offset);
            }

            if (Capacity - _offset < count)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Capacity_NotEnough, value.Length, Capacity - _offset);
            }
        }


        internal static class ThrowHelper
        {
            internal static void ThrowArgumentNullException(ExceptionArgument argument)
            {
                throw new ArgumentNullException(GetArgumentName(argument));
            }

            internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
            {
                throw new ArgumentOutOfRangeException(GetArgumentName(argument));
            }

            internal static void ThrowArgumentException(ExceptionResource resource)
            {
                throw new ArgumentException(GetResourceText(resource));
            }

            internal static void ThrowInvalidOperationException(ExceptionResource resource)
            {
                throw new InvalidOperationException(GetResourceText(resource));
            }

            internal static void ThrowInvalidOperationException(ExceptionResource resource, params object[] args)
            {
                var message = string.Format(GetResourceText(resource), args);

                throw new InvalidOperationException(message);
            }

            internal static ArgumentNullException GetArgumentNullException(ExceptionArgument argument)
            {
                return new ArgumentNullException(GetArgumentName(argument));
            }

            internal static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument));
            }

            internal static ArgumentException GetArgumentException(ExceptionResource resource)
            {
                return new ArgumentException(GetResourceText(resource));
            }

            private static string GetResourceText(ExceptionResource resource)
            {
                return GetResourceName(resource);
                //return Resources.ResourceManager.GetString(GetResourceName(resource), Resources.Culture);
            }

            private static string GetArgumentName(ExceptionArgument argument)
            {
                Debug.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument),
                    "The enum value is not defined, please check the ExceptionArgument Enum.");

                return argument.ToString();
            }

            private static string GetResourceName(ExceptionResource resource)
            {
                Debug.Assert(Enum.IsDefined(typeof(ExceptionResource), resource),
                    "The enum value is not defined, please check the ExceptionResource Enum.");

                return resource.ToString();
            }
        }

        internal enum ExceptionArgument
        {
            buffer,
            offset,
            length,
            text,
            start,
            count,
            index,
            value,
            capacity,
            separators
        }

        internal enum ExceptionResource
        {
            Argument_InvalidOffsetLength,
            Capacity_CannotChangeAfterWriteStarted,
            Capacity_NotEnough,
            Capacity_NotUsedEntirely
        }
    }
}