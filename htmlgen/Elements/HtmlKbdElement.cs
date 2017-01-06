using System.Collections.ObjectModel;

namespace HtmlGenerator
{
    public class HtmlKbdElement : HtmlElement 
    {
        public HtmlKbdElement() : base("kbd", false) 
        {    
        }

        public new HtmlKbdElement WithChild(HtmlElement child) => (HtmlKbdElement)base.WithChild(child);
        public new HtmlKbdElement WithChildren(Collection<HtmlElement> children) => (HtmlKbdElement)base.WithChildren(children);

        public new HtmlKbdElement WithInnerText(string innerText) => (HtmlKbdElement)base.WithInnerText(innerText);

        public new HtmlKbdElement WithAttribute(HtmlAttribute attribute) => (HtmlKbdElement)base.WithAttribute(attribute);
        public new HtmlKbdElement WithAttributes(Collection<HtmlAttribute> attributes) => (HtmlKbdElement)base.WithAttributes(attributes);

		public HtmlKbdElement WithAccessKey(string value) => WithAttribute(Attribute.AccessKey(value));

		public HtmlKbdElement WithClass(string value) => WithAttribute(Attribute.Class(value));

		public HtmlKbdElement WithContentEditable(string value) => WithAttribute(Attribute.ContentEditable(value));

		public HtmlKbdElement WithContextMenu(string value) => WithAttribute(Attribute.ContextMenu(value));

		public HtmlKbdElement WithDir(string value) => WithAttribute(Attribute.Dir(value));

		public HtmlKbdElement WithHidden(string value) => WithAttribute(Attribute.Hidden(value));

		public HtmlKbdElement WithId(string value) => WithAttribute(Attribute.Id(value));

		public HtmlKbdElement WithLang(string value) => WithAttribute(Attribute.Lang(value));

		public HtmlKbdElement WithSpellCheck(string value) => WithAttribute(Attribute.SpellCheck(value));

		public HtmlKbdElement WithStyle(string value) => WithAttribute(Attribute.Style(value));

		public HtmlKbdElement WithTabIndex(string value) => WithAttribute(Attribute.TabIndex(value));
    }
}
