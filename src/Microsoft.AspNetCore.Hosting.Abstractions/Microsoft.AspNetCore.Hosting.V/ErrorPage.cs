using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.RazorViews;
using Microsoft.Extensions.StackTrace.Sources;

namespace Microsoft.AspNetCore.Hosting.Views;

internal class ErrorPage : BaseView
{
	public ErrorPageModel Model { get; set; }

	public ErrorPage(ErrorPageModel model)
	{
		Model = model;
	}

	public ErrorPage()
	{
	}

	public override async Task ExecuteAsync()
	{
		WriteLiteral("\r\n");
		base.Response.ContentType = "text/html; charset=utf-8";
		string value = string.Empty;
		WriteLiteral("<!DOCTYPE html>\r\n<html");
		BeginWriteAttribute("lang", " lang=\"", 422, "\"", 483, 1);
		WriteAttributeValue("", 429, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, 429, 54, yesno: false);
		EndWriteAttribute();
		WriteLiteral(" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n    <head>\r\n        <meta charset=\"utf-8\" />\r\n        <title>");
		Write(Resources.ErrorPageHtml_Title);
		WriteLiteral("</title>\r\n        <style>\r\n            body {\r\n    font-family: 'Segoe UI', Tahoma, Arial, Helvetica, sans-serif;\r\n    font-size: .813em;\r\n    color: #222;\r\n}\r\n\r\nh1, h2, h3, h4, h5 {\r\n    /*font-family: 'Segoe UI',Tahoma,Arial,Helvetica,sans-serif;*/\r\n    font-weight: 100;\r\n}\r\n\r\nh1 {\r\n    color: #44525e;\r\n    margin: 15px 0 15px 0;\r\n}\r\n\r\nh2 {\r\n    margin: 10px 5px 0 0;\r\n}\r\n\r\nh3 {\r\n    color: #363636;\r\n    margin: 5px 5px 0 0;\r\n}\r\n\r\ncode {\r\n    font-family: Consolas, \"Courier New\", courier, monospace;\r\n}\r\n\r\nbody .titleerror {\r\n    padding: 3px 3px 6px 3px;\r\n    display: block;\r\n    font-size: 1.5em;\r\n    font-weight: 100;\r\n}\r\n\r\nbody .location {\r\n    margin: 3px 0 10px 30px;\r\n}\r\n\r\n#header {\r\n    font-size: 18px;\r\n    padding: 15px 0;\r\n    border-top: 1px #ddd solid;\r\n    border-bottom: 1px #ddd solid;\r\n    margin-bottom: 0;\r\n}\r\n\r\n    #header li {\r\n        display: inline;\r\n        margin: 5px;\r\n        padding: 5px;\r\n        color: #a0a0a0;\r\n        cursor: pointer;\r\n    }\r\n\r\n    #header .selected {\r\n        ba");
		WriteLiteral("ckground: #44c5f2;\r\n        color: #fff;\r\n    }\r\n\r\n#stackpage ul {\r\n    list-style: none;\r\n    padding-left: 0;\r\n    margin: 0;\r\n    /*border-bottom: 1px #ddd solid;*/\r\n}\r\n\r\n#stackpage .details {\r\n    font-size: 1.2em;\r\n    padding: 3px;\r\n    color: #000;\r\n}\r\n\r\n#stackpage .stackerror {\r\n    padding: 5px;\r\n    border-bottom: 1px #ddd solid;\r\n}\r\n\r\n\r\n#stackpage .frame {\r\n    padding: 0;\r\n    margin: 0 0 0 30px;\r\n}\r\n\r\n    #stackpage .frame h3 {\r\n        padding: 2px;\r\n        margin: 0;\r\n    }\r\n\r\n#stackpage .source {\r\n    padding: 0 0 0 30px;\r\n}\r\n\r\n    #stackpage .source ol li {\r\n        font-family: Consolas, \"Courier New\", courier, monospace;\r\n        white-space: pre;\r\n        background-color: #fbfbfb;\r\n    }\r\n\r\n#stackpage .frame .source .highlight li span {\r\n    color: #FF0000;\r\n}\r\n\r\n#stackpage .source ol.collapsible li {\r\n    color: #888;\r\n}\r\n\r\n    #stackpage .source ol.collapsible li span {\r\n        color: #606060;\r\n    }\r\n\r\n.page table {\r\n    border-collapse: separate;\r\n    border-spacing: 0;\r\n    margin:");
		WriteLiteral(" 0 0 20px;\r\n}\r\n\r\n.page th {\r\n    vertical-align: bottom;\r\n    padding: 10px 5px 5px 5px;\r\n    font-weight: 400;\r\n    color: #a0a0a0;\r\n    text-align: left;\r\n}\r\n\r\n.page td {\r\n    padding: 3px 10px;\r\n}\r\n\r\n.page th, .page td {\r\n    border-right: 1px #ddd solid;\r\n    border-bottom: 1px #ddd solid;\r\n    border-left: 1px transparent solid;\r\n    border-top: 1px transparent solid;\r\n    box-sizing: border-box;\r\n}\r\n\r\n    .page th:last-child, .page td:last-child {\r\n        border-right: 1px transparent solid;\r\n    }\r\n\r\n.page .length {\r\n    text-align: right;\r\n}\r\n\r\na {\r\n    color: #1ba1e2;\r\n    text-decoration: none;\r\n}\r\n\r\n    a:hover {\r\n        color: #13709e;\r\n        text-decoration: underline;\r\n    }\r\n\r\n.showRawException {\r\n    cursor: pointer;\r\n    color: #44c5f2;\r\n    background-color: transparent;\r\n    font-size: 1.2em;\r\n    text-align: left;\r\n    text-decoration: none;\r\n    display: inline-block;\r\n    border: 0;\r\n    padding: 0;\r\n}\r\n\r\n.rawExceptionStackTrace {\r\n    font-size: 1.2em;\r\n}\r\n\r\n.rawExceptionBlock {\r\n  ");
		WriteLiteral("  border-top: 1px #ddd solid;\r\n    border-bottom: 1px #ddd solid;\r\n}\r\n\r\n.showRawExceptionContainer {\r\n    margin-top: 10px;\r\n    margin-bottom: 10px;\r\n}\r\n\r\n.expandCollapseButton {\r\n    cursor: pointer;\r\n    float: left;\r\n    height: 16px;\r\n    width: 16px;\r\n    font-size: 10px;\r\n    position: absolute;\r\n    left: 10px;\r\n    background-color: #eee;\r\n    padding: 0;\r\n    border: 0;\r\n    margin: 0;\r\n}\r\n\r\n        </style>\r\n    </head>\r\n    <body>\r\n        <h1>");
		Write(Resources.ErrorPageHtml_UnhandledException);
		WriteLiteral("</h1>\r\n");
		foreach (ExceptionDetails errorDetail in Model.ErrorDetails)
		{
			WriteLiteral("            <div class=\"titleerror\">");
			Write(errorDetail.Error.GetType().Name);
			WriteLiteral(": ");
			base.Output.Write(HtmlEncodeAndReplaceLineBreaks(errorDetail.Error.Message));
			WriteLiteral("</div>\r\n");
			StackFrameSourceCodeInfo stackFrameSourceCodeInfo = errorDetail.StackFrames.FirstOrDefault();
			if (stackFrameSourceCodeInfo != null)
			{
				value = stackFrameSourceCodeInfo.Function;
			}
			if (!string.IsNullOrEmpty(value) && stackFrameSourceCodeInfo != null && !string.IsNullOrEmpty(stackFrameSourceCodeInfo.File))
			{
				WriteLiteral("                <p class=\"location\">");
				Write(value);
				WriteLiteral(" in <code");
				BeginWriteAttribute("title", " title=\"", 4844, "\"", 4868, 1);
				WriteAttributeValue("", 4852, stackFrameSourceCodeInfo.File, 4852, 16, yesno: false);
				EndWriteAttribute();
				WriteLiteral(">");
				Write(Path.GetFileName(stackFrameSourceCodeInfo.File));
				WriteLiteral("</code>, line ");
				Write(stackFrameSourceCodeInfo.Line);
				WriteLiteral("</p>\r\n");
			}
			else if (!string.IsNullOrEmpty(value))
			{
				WriteLiteral("                <p class=\"location\">");
				Write(value);
				WriteLiteral("</p>\r\n");
			}
			else
			{
				WriteLiteral("                <p class=\"location\">");
				Write(Resources.ErrorPageHtml_UnknownLocation);
				WriteLiteral("</p>\r\n");
			}
			if (errorDetail.Error is ReflectionTypeLoadException ex && ex.LoaderExceptions.Length != 0)
			{
				WriteLiteral("                    <h3>Loader Exceptions:</h3>\r\n                    <ul>\r\n");
				Exception[] loaderExceptions = ex.LoaderExceptions;
				foreach (Exception ex2 in loaderExceptions)
				{
					WriteLiteral("                            <li>");
					Write(ex2.Message);
					WriteLiteral("</li>\r\n");
				}
				WriteLiteral("                    </ul>\r\n");
			}
		}
		WriteLiteral("        <div id=\"stackpage\" class=\"page\">\r\n            <ul>\r\n");
		int num = 0;
		int num2 = 0;
		WriteLiteral("                ");
		foreach (ExceptionDetails errorDetail2 in Model.ErrorDetails)
		{
			num++;
			string value2 = "exceptionDetail" + num;
			WriteLiteral("                    <li>\r\n                        <h2 class=\"stackerror\">");
			Write(errorDetail2.Error.GetType().Name);
			WriteLiteral(": ");
			Write(errorDetail2.Error.Message);
			WriteLiteral("</h2>\r\n                        <ul>\r\n");
			foreach (StackFrameSourceCodeInfo stackFrame in errorDetail2.StackFrames)
			{
				num2++;
				string value3 = "frame" + num2;
				WriteLiteral("                            <li class=\"frame\"");
				BeginWriteAttribute("id", " id=\"", 6874, "\"", 6887, 1);
				WriteAttributeValue("", 6879, value3, 6879, 8, yesno: false);
				EndWriteAttribute();
				WriteLiteral(">\r\n");
				if (string.IsNullOrEmpty(stackFrame.File))
				{
					WriteLiteral("                                    <h3>");
					Write(stackFrame.Function);
					WriteLiteral("</h3>\r\n");
				}
				else
				{
					WriteLiteral("                                    <h3>");
					Write(stackFrame.Function);
					WriteLiteral(" in <code");
					BeginWriteAttribute("title", " title=\"", 7232, "\"", 7251, 1);
					WriteAttributeValue("", 7240, stackFrame.File, 7240, 11, yesno: false);
					EndWriteAttribute();
					WriteLiteral(">");
					Write(Path.GetFileName(stackFrame.File));
					WriteLiteral("</code></h3>\r\n");
				}
				WriteLiteral("\r\n");
				if (stackFrame.Line != 0 && stackFrame.ContextCode.Any())
				{
					WriteLiteral("                                    <button class=\"expandCollapseButton\" data-frameId=\"");
					Write(value3);
					WriteLiteral("\">+</button>\r\n                                    <div class=\"source\">\r\n");
					if (stackFrame.PreContextCode.Any())
					{
						WriteLiteral("                                            <ol");
						BeginWriteAttribute("start", " start=\"", 7791, "\"", 7820, 1);
						WriteAttributeValue("", 7799, stackFrame.PreContextLine, 7799, 21, yesno: false);
						EndWriteAttribute();
						WriteLiteral(" class=\"collapsible\">\r\n");
						foreach (string item in stackFrame.PreContextCode)
						{
							WriteLiteral("                                                    <li><span>");
							Write(item);
							WriteLiteral("</span></li>\r\n");
						}
						WriteLiteral("                                            </ol>\r\n");
					}
					WriteLiteral("\r\n                                        <ol");
					BeginWriteAttribute("start", " start=\"", 8259, "\"", 8278, 1);
					WriteAttributeValue("", 8267, stackFrame.Line, 8267, 11, yesno: false);
					EndWriteAttribute();
					WriteLiteral(" class=\"highlight\">\r\n");
					foreach (string item2 in stackFrame.ContextCode)
					{
						WriteLiteral("                                                <li><span>");
						Write(item2);
						WriteLiteral("</span></li>\r\n");
					}
					WriteLiteral("                                        </ol>\r\n\r\n");
					if (stackFrame.PostContextCode.Any())
					{
						WriteLiteral("                                            <ol");
						BeginWriteAttribute("start", " start='", 8771, "'", 8796, 1);
						WriteAttributeValue("", 8779, stackFrame.Line + 1, 8779, 17, yesno: false);
						EndWriteAttribute();
						WriteLiteral(" class=\"collapsible\">\r\n");
						foreach (string item3 in stackFrame.PostContextCode)
						{
							WriteLiteral("                                                    <li><span>");
							Write(item3);
							WriteLiteral("</span></li>\r\n");
						}
						WriteLiteral("                                            </ol>\r\n");
					}
					WriteLiteral("                                    </div>\r\n");
				}
				WriteLiteral("                            </li>\r\n");
			}
			WriteLiteral("                        </ul>\r\n                    </li>\r\n                    <li>\r\n                        <br/>\r\n                        <div class=\"rawExceptionBlock\">\r\n                            <div class=\"showRawExceptionContainer\">\r\n                                <button class=\"showRawException\" data-exceptionDetailId=\"");
			Write(value2);
			WriteLiteral("\">Show raw exception details</button>\r\n                            </div>\r\n                            <div");
			BeginWriteAttribute("id", " id=\"", 9787, "\"", 9810, 1);
			WriteAttributeValue("", 9792, value2, 9792, 18, yesno: false);
			EndWriteAttribute();
			WriteLiteral(" class=\"rawExceptionDetails\">\r\n                                <pre class=\"rawExceptionStackTrace\">");
			Write(errorDetail2.Error.ToString());
			WriteLiteral("</pre>\r\n                            </div>\r\n                        </div>\r\n                    </li>\r\n");
		}
		WriteLiteral("            </ul>\r\n        </div>\r\n        <footer>\r\n            ");
		Write(Model.RuntimeDisplayName);
		WriteLiteral(" ");
		Write(Model.RuntimeArchitecture);
		WriteLiteral(" v");
		Write(Model.ClrVersion);
		WriteLiteral(" &nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;Microsoft.AspNetCore.Hosting version ");
		Write(Model.CurrentAssemblyVesion);
		WriteLiteral(" &nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp; ");
		Write(Model.OperatingSystemDescription);
		WriteLiteral(" &nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;<a href=\"http://go.microsoft.com/fwlink/?LinkId=517394\">Need help?</a>\r\n        </footer>\r\n        <script>\r\n            //<!--\r\n            (function (window, undefined) {\r\n    \"use strict\";\r\n\r\n    function ns(selector, element) {\r\n        return new NodeCollection(selector, element);\r\n    }\r\n\r\n    function NodeCollection(selector, element) {\r\n        this.items = [];\r\n        element = element || window.document;\r\n\r\n        var nodeList;\r\n\r\n        if (typeof (selector) === \"string\") {\r\n            nodeList = element.querySelectorAll(selector);\r\n            for (var i = 0, l = nodeList.length; i < l; i++) {\r\n                this.items.push(nodeList.item(i));\r\n            }\r\n        }\r\n    }\r\n\r\n    NodeCollection.prototype = {\r\n        each: function (callback) {\r\n            for (var i = 0, l = this.items.length; i < l; i++) {\r\n                callback(this.items[i], i);\r\n            }\r\n            return this;\r\n        },\r\n\r\n        children: function (selector) {\r\n   ");
		WriteLiteral("         var children = [];\r\n\r\n            this.each(function (el) {\r\n                children = children.concat(ns(selector, el).items);\r\n            });\r\n\r\n            return ns(children);\r\n        },\r\n\r\n        hide: function () {\r\n            this.each(function (el) {\r\n                el.style.display = \"none\";\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        toggle: function () {\r\n            this.each(function (el) {\r\n                el.style.display = el.style.display === \"none\" ? \"\" : \"none\";\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        show: function () {\r\n            this.each(function (el) {\r\n                el.style.display = \"\";\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        addClass: function (className) {\r\n            this.each(function (el) {\r\n                var existingClassName = el.className,\r\n                    classNames;\r\n                if (!existingClassName) {\r\n                    el.className = className;\r\n             ");
		WriteLiteral("   } else {\r\n                    classNames = existingClassName.split(\" \");\r\n                    if (classNames.indexOf(className) < 0) {\r\n                        el.className = existingClassName + \" \" + className;\r\n                    }\r\n                }\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        removeClass: function (className) {\r\n            this.each(function (el) {\r\n                var existingClassName = el.className,\r\n                    classNames, index;\r\n                if (existingClassName === className) {\r\n                    el.className = \"\";\r\n                } else if (existingClassName) {\r\n                    classNames = existingClassName.split(\" \");\r\n                    index = classNames.indexOf(className);\r\n                    if (index > 0) {\r\n                        classNames.splice(index, 1);\r\n                        el.className = classNames.join(\" \");\r\n                    }\r\n                }\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n    ");
		WriteLiteral("    attr: function (name) {\r\n            if (this.items.length === 0) {\r\n                return null;\r\n            }\r\n\r\n            return this.items[0].getAttribute(name);\r\n        },\r\n\r\n        on: function (eventName, handler) {\r\n            this.each(function (el, idx) {\r\n                var callback = function (e) {\r\n                    e = e || window.event;\r\n                    if (!e.which && e.keyCode) {\r\n                        e.which = e.keyCode; // Normalize IE8 key events\r\n                    }\r\n                    handler.apply(el, [e]);\r\n                };\r\n\r\n                if (el.addEventListener) { // DOM Events\r\n                    el.addEventListener(eventName, callback, false);\r\n                } else if (el.attachEvent) { // IE8 events\r\n                    el.attachEvent(\"on\" + eventName, callback);\r\n                } else {\r\n                    el[\"on\" + type] = callback;\r\n                }\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        click: function (handler) {\r");
		WriteLiteral("\n            return this.on(\"click\", handler);\r\n        },\r\n\r\n        keypress: function (handler) {\r\n            return this.on(\"keypress\", handler);\r\n        }\r\n    };\r\n\r\n    function frame(el) {\r\n        ns(\".source .collapsible\", el).toggle();\r\n    }\r\n\r\n    function expandCollapseButton(el) {\r\n        var frameId = el.getAttribute(\"data-frameId\");\r\n        frame(document.getElementById(frameId));\r\n        if (el.innerText === \"+\") {\r\n            el.innerText = \"-\";\r\n        }\r\n        else {\r\n            el.innerText = \"+\";\r\n        }\r\n    }\r\n\r\n    function tab(el) {\r\n        var unselected = ns(\"#header .selected\").removeClass(\"selected\").attr(\"id\");\r\n        var selected = ns(\"#\" + el.id).addClass(\"selected\").attr(\"id\");\r\n\r\n        ns(\"#\" + unselected + \"page\").hide();\r\n        ns(\"#\" + selected + \"page\").show();\r\n    }\r\n\r\n    ns(\".rawExceptionDetails\").hide();\r\n    ns(\".collapsible\").hide();\r\n    ns(\".page\").hide();\r\n    ns(\"#stackpage\").show();\r\n\r\n    ns(\".expandCollapseButton\")\r\n        .click(functi");
		WriteLiteral("on () {\r\n            expandCollapseButton(this);\r\n        })\r\n        .keypress(function (e) {\r\n            if (e.which === 13) {\r\n                expandCollapseButton(this);\r\n            }\r\n        });\r\n\r\n    ns(\"#header li\")\r\n        .click(function () {\r\n            tab(this);\r\n        })\r\n        .keypress(function (e) {\r\n            if (e.which === 13) {\r\n                tab(this);\r\n            }\r\n        });\r\n\r\n    ns(\".showRawException\")\r\n        .click(function () {\r\n            var exceptionDetailId = this.getAttribute(\"data-exceptionDetailId\");\r\n            ns(\"#\" + exceptionDetailId).toggle();\r\n        });\r\n})(window);\r\n            //-->\r\n        </script>\r\n</body>\r\n</html>\r\n");
	}
}
