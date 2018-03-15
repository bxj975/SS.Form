﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SS.Form.Model;

namespace SS.Form.Core
{
    public class Utils
    {
        public static readonly Color[] Colors = { Color.FromArgb(37, 72, 91), Color.FromArgb(68, 24, 25), Color.FromArgb(17, 46, 2), Color.FromArgb(70, 16, 100), Color.FromArgb(24, 88, 74) };

        public static bool EqualsIgnoreCase(string a, string b)
        {
            if (a == b) return true;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            return string.Equals(a.Trim().ToLower(), b.Trim().ToLower());
        }

        public static DateTime ToDateTime(string dateTimeStr)
        {
            return ToDateTime(dateTimeStr, DateTime.Now);
        }

        public static DateTime ToDateTime(string dateTimeStr, DateTime defaultValue)
        {
            var datetime = defaultValue;
            if (!string.IsNullOrEmpty(dateTimeStr))
            {
                if (!DateTime.TryParse(dateTimeStr.Trim(), out datetime))
                {
                    datetime = defaultValue;
                }
                return datetime;
            }
            if (datetime <= DateTime.MinValue)
            {
                datetime = DateTime.Now;
            }
            return datetime;
        }

        public static bool ToBool(string boolStr)
        {
            bool boolean;
            if (!bool.TryParse(boolStr?.Trim(), out boolean))
            {
                boolean = false;
            }
            return boolean;
        }

        public static int ToInt(string intStr)
        {
            int i;
            if (!int.TryParse(intStr?.Trim(), out i))
            {
                i = 0;
            }
            return i;
        }

        public static string GetMessageHtml(string message, bool isSuccess)
        {
            return isSuccess
                ? $@"<div class=""alert alert-success"" role=""alert"">{message}</div>"
                : $@"<div class=""alert alert-danger"" role=""alert"">{message}</div>";
        }

        public static string ReplaceNewline(string inputString, string replacement)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;
            var retVal = new StringBuilder();
            inputString = inputString.Trim();
            foreach (var t in inputString)
            {
                switch (t)
                {
                    case '\n':
                        retVal.Append(replacement);
                        break;
                    case '\r':
                        break;
                    default:
                        retVal.Append(t);
                        break;
                }
            }
            return retVal.ToString();
        }

        public static string GetSelectedListControlValueCollection(ListControl listControl)
        {
            var list = new List<string>();
            if (listControl != null)
            {
                foreach (ListItem item in listControl.Items)
                {
                    if (item.Selected)
                    {
                        list.Add(item.Value);
                    }
                }
            }
            return string.Join(",", list);
        }

        public static object Eval(object dataItem, string name)
        {
            object o = null;
            try
            {
                o = DataBinder.Eval(dataItem, name);
            }
            catch
            {
                // ignored
            }
            if (o == DBNull.Value)
            {
                o = null;
            }
            return o;
        }

        public static int EvalInt(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        public static decimal EvalDecimal(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o == null ? 0 : Convert.ToDecimal(o);
        }

        public static string EvalString(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o?.ToString() ?? string.Empty;
        }

        public static DateTime EvalDateTime(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            if (o == null)
            {
                return DateTime.MinValue;
            }
            return (DateTime)o;
        }

        public static bool EvalBool(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o != null && Convert.ToBoolean(o.ToString());
        }

        public static bool IsSelectFieldType(string inputType)
        {
            return EqualsIgnoreCase(inputType, nameof(FieldType.CheckBox)) ||
                   EqualsIgnoreCase(inputType, nameof(FieldType.Radio)) ||
                   EqualsIgnoreCase(inputType, nameof(FieldType.SelectMultiple)) ||
                   EqualsIgnoreCase(inputType, nameof(FieldType.SelectOne));
        }

        public static string GetPostMessageScript(int siteId, int channelId, int contentId, bool isSuccess)
        {
            var containerId = $"stl_form_{siteId}_{channelId}_{contentId}";
            return $"<script>window.parent.postMessage({{containerId: '{containerId}', isSuccess: {isSuccess.ToString().ToLower()}}}, '*');</script>";
        }

        public static string GetControlRenderHtml(Control control)
        {
            var builder = new StringBuilder();
            if (control != null)
            {
                var sw = new StringWriter(builder);
                var htw = new HtmlTextWriter(sw);
                control.RenderControl(htw);
            }
            return builder.ToString();
        }

        public static List<string> StringCollectionToStringList(string collection)
        {
            return StringCollectionToStringList(collection, ',');
        }

        public static List<string> StringCollectionToStringList(string collection, char split)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(collection))
            {
                var array = collection.Split(split);
                foreach (var s in array)
                {
                    list.Add(s);
                }
            }
            return list;
        }

        public static List<string> GetHtmlFormElements(string content)
        {
            var list = new List<string>();

            const RegexOptions options = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase;

            var regex = "<input\\s*[^>]*?/>|<input\\s*[^>]*?>[^>]*?</input>";
            var reg = new Regex(regex, options);
            var mc = reg.Matches(content);
            for (var i = 0; i < mc.Count; i++)
            {
                var element = mc[i].Value;
                list.Add(element);
            }

            regex = "<textarea\\s*[^>]*?/>|<textarea\\s*[^>]*?>[^>]*?</textarea>";
            reg = new Regex(regex, options);
            mc = reg.Matches(content);
            for (var i = 0; i < mc.Count; i++)
            {
                var element = mc[i].Value;
                list.Add(element);
            }

            regex = "<select\\b[\\s\\S]*?</select>";
            reg = new Regex(regex, options);
            mc = reg.Matches(content);
            for (var i = 0; i < mc.Count; i++)
            {
                var element = mc[i].Value;
                list.Add(element);
            }

            return list;
        }

        public static string ToAttributesString(NameValueCollection attributes)
        {
            var builder = new StringBuilder();
            if (attributes != null && attributes.Count > 0)
            {
                foreach (string key in attributes.Keys)
                {
                    var value = attributes[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.Replace("\"", "'");
                    }
                    builder.Append($@"{key}=""{value}"" ");
                }
                builder.Length--;
            }
            return builder.ToString();
        }

        public static string ToAttributesString(StringDictionary attributes)
        {
            var builder = new StringBuilder();
            if (attributes != null && attributes.Count > 0)
            {
                foreach (string key in attributes.Keys)
                {
                    var value = attributes[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.Replace("\"", "'");
                    }
                    builder.Append($@"{key}=""{value}"" ");
                }
                builder.Length--;
            }
            return builder.ToString();
        }

        private const string XmlDeclaration = "<?xml version='1.0'?>";

        private const string XmlNamespaceStart = "<root>";

        private const string XmlNamespaceEnd = "</root>";

        public static XmlDocument GetXmlDocument(string element, bool isXml)
        {
            var xmlDocument = new XmlDocument
            {
                PreserveWhitespace = true
            };
            try
            {
                if (isXml)
                {
                    xmlDocument.LoadXml(XmlDeclaration + XmlNamespaceStart + element + XmlNamespaceEnd);
                }
                else
                {
                    xmlDocument.LoadXml(XmlDeclaration + XmlNamespaceStart + Main.Instance.ParseApi.HtmlToXml(element) + XmlNamespaceEnd);
                }
            }
            catch
            {
                // ignored
            }
            //catch(Exception e)
            //{
            //    TraceUtils.Warn(e.ToString());
            //    throw e;
            //}
            return xmlDocument;
        }

        public static void ParseHtmlElement(string htmlElement, out string tagName, out string innerXml, out NameValueCollection attributes)
        {
            tagName = string.Empty;
            innerXml = string.Empty;
            attributes = new NameValueCollection();

            var document = GetXmlDocument(htmlElement, false);
            XmlNode elementNode = document.DocumentElement;
            if (elementNode == null) return;

            elementNode = elementNode.FirstChild;
            tagName = elementNode.Name;
            innerXml = elementNode.InnerXml;
            if (elementNode.Attributes == null) return;

            var elementIe = elementNode.Attributes.GetEnumerator();
            while (elementIe.MoveNext())
            {
                var attr = (XmlAttribute)elementIe.Current;
                if (attr != null)
                {
                    var attributeName = attr.Name;
                    attributes.Add(attributeName, attr.Value);
                }
            }
        }

        public static string GetHtmlElementById(string html, string id)
        {
            const RegexOptions options = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase;

            var regex = $"<input\\s*[^>]*?id\\s*=\\s*(\"{id}\"|\'{id}\'|{id}).*?>";
            var reg = new Regex(regex, options);
            var match = reg.Match(html);
            if (match.Success)
            {
                return match.Value;
            }

            regex = $"<\\w+\\s*[^>]*?id\\s*=\\s*(\"{id}\"|\'{id}\'|{id})[^>]*/\\s*>";
            reg = new Regex(regex, options);
            match = reg.Match(html);
            if (match.Success)
            {
                return match.Value;
            }

            regex = $"<(\\w+?)\\s*[^>]*?id\\s*=\\s*(\"{id}\"|\'{id}\'|{id}).*?>[^>]*</\\1[^>]*>";
            reg = new Regex(regex, options);
            match = reg.Match(html);
            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }

        public static string GetHtmlElementByRole(string html, string role)
        {
            const RegexOptions options = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase;

            var regex = $"<input\\s*[^>]*?role\\s*=\\s*(\"{role}\"|\'{role}\'|{role}).*?>";
            var reg = new Regex(regex, options);
            var match = reg.Match(html);
            if (match.Success)
            {
                return match.Value;
            }

            regex = $"<\\w+\\s*[^>]*?role\\s*=\\s*(\"{role}\"|\'{role}\'|{role})[^>]*/\\s*>";
            reg = new Regex(regex, options);
            match = reg.Match(html);
            if (match.Success)
            {
                return match.Value;
            }

            regex = $"<(\\w+?)\\s*[^>]*?role\\s*=\\s*(\"{role}\"|\'{role}\'|{role}).*?>[^>]*</\\1[^>]*>";
            reg = new Regex(regex, options);
            match = reg.Match(html);
            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }

        public static string ToStringWithQuote(List<string> collection)
        {
            var builder = new StringBuilder();
            if (collection != null)
            {
                foreach (var obj in collection)
                {
                    builder.Append("'").Append(obj).Append("'").Append(",");
                }
                if (builder.Length != 0) builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public static void RewriteSubmitButton(StringBuilder builder, string clickString)
        {
            var submitElement = GetHtmlElementByRole(builder.ToString(), "submit");
            if (string.IsNullOrEmpty(submitElement))
            {
                submitElement = GetHtmlElementById(builder.ToString(), "submit");
            }
            if (!string.IsNullOrEmpty(submitElement))
            {
                var document = GetXmlDocument(submitElement, false);
                XmlNode elementNode = document.DocumentElement;
                if (elementNode != null)
                {
                    elementNode = elementNode.FirstChild;
                    if (elementNode.Attributes != null)
                    {
                        var elementIe = elementNode.Attributes.GetEnumerator();
                        var attributes = new StringDictionary();
                        while (elementIe.MoveNext())
                        {
                            var attr = (XmlAttribute)elementIe.Current;
                            if (attr != null)
                            {
                                var attributeName = attr.Name.ToLower();
                                if (attributeName == "href")
                                {
                                    attributes.Add(attr.Name, "javascript:;");
                                }
                                else if (attributeName != "onclick")
                                {
                                    attributes.Add(attr.Name, attr.Value);
                                }
                            }
                        }
                        attributes.Add("onclick", clickString);
                        attributes.Remove("id");
                        attributes.Remove("name");

                        //attributes.Add("id", "submit_" + styleID);

                        if (EqualsIgnoreCase(elementNode.Name, "a"))
                        {
                            attributes.Remove("href");
                            attributes.Add("href", "javascript:;");
                        }

                        if (!string.IsNullOrEmpty(elementNode.InnerXml))
                        {
                            builder.Replace(submitElement,
                                $@"<{elementNode.Name} {ToAttributesString(attributes)}>{elementNode.InnerXml}</{elementNode
                                    .Name}>");
                        }
                        else
                        {
                            builder.Replace(submitElement,
                                $@"<{elementNode.Name} {ToAttributesString(attributes)}/>");
                        }
                    }
                }
            }
        }

        public static string JsonSerialize(object obj)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var timeFormat = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
                settings.Converters.Add(timeFormat);

                return JsonConvert.SerializeObject(obj, settings);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static T JsonDeserialize<T>(string json)
        {
            try
            {
                var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var timeFormat = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
                settings.Converters.Add(timeFormat);

                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch
            {
                return default(T);
            }
        }

        public static string ReadText(string filePath)
        {
            var sr = new StreamReader(filePath, Encoding.UTF8);
            var text = sr.ReadToEnd();
            sr.Close();
            return text;
        }

        public static void SelectSingleItem(ListControl listControl, string value)
        {
            if (listControl == null) return;

            listControl.ClearSelection();

            foreach (ListItem item in listControl.Items)
            {
                if (string.Equals(item.Value, value))
                {
                    item.Selected = true;
                    break;
                }
            }
        }

        public static void SelectSingleItemIgnoreCase(ListControl listControl, string value)
        {
            if (listControl == null) return;

            listControl.ClearSelection();
            foreach (ListItem item in listControl.Items)
            {
                if (EqualsIgnoreCase(item.Value, value))
                {
                    item.Selected = true;
                    break;
                }
            }
        }

        public static void SelectMultiItems(ListControl listControl, params string[] values)
        {
            if (listControl == null) return;

            listControl.ClearSelection();
            foreach (ListItem item in listControl.Items)
            {
                foreach (var value in values)
                {
                    if (string.Equals(item.Value, value))
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }

        public static void SelectMultiItems(ListControl listControl, List<string> values)
        {
            if (listControl == null) return;

            listControl.ClearSelection();
            foreach (ListItem item in listControl.Items)
            {
                foreach (var value in values)
                {
                    if (string.Equals(item.Value, value))
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }

        public static void SelectMultiItems(ListControl listControl, List<int> values)
        {
            if (listControl == null) return;

            listControl.ClearSelection();
            foreach (ListItem item in listControl.Items)
            {
                foreach (var intVal in values)
                {
                    if (string.Equals(item.Value, intVal.ToString()))
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }

        public static void SelectMultiItemsIgnoreCase(ListControl listControl, params string[] values)
        {
            if (listControl == null) return;

            listControl.ClearSelection();
            foreach (ListItem item in listControl.Items)
            {
                foreach (var value in values)
                {
                    if (EqualsIgnoreCase(item.Value, value))
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }

        public static string[] GetDirectoryNames(string directoryPath)
        {
            var directorys = Directory.GetDirectories(directoryPath);
            var retval = new string[directorys.Length];
            var i = 0;
            foreach (var directory in directorys)
            {
                var directoryInfo = new DirectoryInfo(directory);
                retval[i++] = directoryInfo.Name;
            }
            return retval;
        }
    }
}
