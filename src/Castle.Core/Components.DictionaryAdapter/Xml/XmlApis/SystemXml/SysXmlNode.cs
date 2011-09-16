﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.f
// See the License for the specific language governing permissions and
// limitations under the License.

#if !SILVERLIGHT
namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
	using System.Xml;
	using System.Xml.XPath;

	public class SysXmlNode : IXmlNode,
		ILazy<XmlNode>,
		ILazy<XPathNavigator>
	{
		protected XmlNode node;
		protected Type    type;

		protected SysXmlNode() { }

		public SysXmlNode(XmlNode node, Type type)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			if (type == null)
				throw new ArgumentNullException("type");

			this.node = node;
			this.type = type;
		}

		public virtual bool Exists
		{
			get { return true; }
		}

		public virtual Type ClrType
		{
			get { return type; }
		}

		public virtual string LocalName
		{
			get { return node.LocalName; }
		}

		public virtual string NamespaceUri
		{
			get { return node.NamespaceURI; }
		}

		public virtual string XsiType
		{
			get { return IsElement ? node.GetXsiType() : null; }
		}

		public virtual bool IsElement
		{
			get { return node.NodeType == XmlNodeType.Element; }
		}

		public virtual bool IsAttribute
		{
			get { return node.NodeType == XmlNodeType.Attribute; }
		}

		public virtual bool IsRoot
		{
			get { return node.NodeType == XmlNodeType.Document; }
		}

		public virtual bool IsNil
		{
			get { return IsElement && node.IsXsiNil(); }
			set { RequireElement(); node.SetXsiNil(value); }
		}

		public virtual string Value
		{
			get { return node.InnerText; }
			set { node.InnerText = value; }
		}

		public virtual string Xml
		{
			get { return node.OuterXml; }
		}

		public IXmlCursor SelectSelf(Type clrType)
		{
			return new XmlSelfCursor(this, clrType);
		}

		public IXmlCursor SelectChildren(IXmlKnownTypeMap knownTypes, CursorFlags flags)
		{
			return new SysXmlCursor(this, knownTypes, flags);
		}

		public XmlReader ReadSubtree()
		{
			return node.CreateNavigator().ReadSubtree();
		}

		public XmlWriter WriteAttributes()
		{
			return node.CreateNavigator().CreateAttributes();
		}

		public XmlWriter WriteChildren()
		{
			return node.CreateNavigator().AppendChild();
		}

		public IXmlCursor Select(ICompiledPath path, IXmlIncludedTypeMap includedTypes, CursorFlags flags)
		{
			return flags.SupportsMutation()
				? (IXmlCursor) new XPathMutableCursor (this, path, includedTypes, flags)
				: (IXmlCursor) new XPathReadOnlyCursor(this, path, includedTypes, flags);
		}

		public object Evaluate(ICompiledPath path)
		{
			return node.CreateNavigator().Evaluate(path.Expression);
		}

		public XmlNode GetNode()
		{
			return node;
		}

		public virtual void Realize()
		{
			// Default nodes are realized already
		}

		public void Clear()
		{
			if (IsElement)
				ClearAttributes();
			ClearChildren();
		}

		private void ClearChildren()
		{
			XmlNode next;
			for (var n = node.FirstChild; n != null; n = next)
			{
				next = n.NextSibling;
				node.RemoveChild(node);
			}
		}

		private void ClearAttributes()
		{
			var attributes = node.Attributes;
			var count = attributes.Count;
			while (count > 0)
			{
				var attribute = attributes[--count];
				if (!attribute.IsNamespace() && !attribute.IsXsiType())
					attributes.RemoveAt(count);
			}
		}

		private void RequireElement()
		{
			if (!IsElement)
				throw Error.OperationNotValidOnAttribute();
		}

		public bool HasValue
		{
			get { return Exists; }
		}

		XmlNode ILazy<XmlNode>.Value
		{
			get { Realize(); return node; }
		}

		XPathNavigator ILazy<XPathNavigator>.Value
		{
			get { Realize(); return node.CreateNavigator(); }
		}
	}
}
#endif