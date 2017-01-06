using System.Collections.ObjectModel;

namespace HtmlGenerator
{
    public class HtmlLiElement : HtmlElement 
    {
        public HtmlLiElement() : base("li", false) 
        {    
        }

        public new HtmlLiElement WithChild(HtmlElement child) => (HtmlLiElement)base.WithChild(child);
        public new HtmlLiElement WithChildren(Collection<HtmlElement> children) => (HtmlLiElement)base.WithChildren(children);

        public new HtmlLiElement WithInnerText(string innerText) => (HtmlLiElement)base.WithInnerText(innerText);

        public new HtmlLiElement WithAttribute(HtmlAttribute attribute) => (HtmlLiElement)base.WithAttribute(attribute);
        public new HtmlLiElement WithAttributes(Collection<HtmlAttribute> attributes) => (HtmlLiElement)base.WithAttributes(attributes);

		public HtmlLiElement WithValue(string value) => WithAttribute(Attribute.Value(value));

		public HtmlLiElement WithAccessKey(string value) => WithAttribute(Attribute.AccessKey(value));

		public HtmlLiElement WithClass(string value) => WithAttribute(Attribute.Class(value));

		public HtmlLiElement WithContentEditable(string value) => WithAttribute(Attribute.ContentEditable(value));

		public HtmlLiElement WithContextMenu(string value) => WithAttribute(Attribute.ContextMenu(value));

		public HtmlLiElement WithDir(string value) => WithAttribute(Attribute.Dir(value));

		public HtmlLiElement WithHidden(string value) => WithAttribute(Attribute.Hidden(value));

		public HtmlLiElement WithId(string value) => WithAttribute(Attribute.Id(value));

		public HtmlLiElement WithLang(string value) => WithAttribute(Attribute.Lang(value));

		public HtmlLiElement WithSpellCheck(string value) => WithAttribute(Attribute.SpellCheck(value));

		public HtmlLiElement WithStyle(string value) => WithAttribute(Attribute.Style(value));

		public HtmlLiElement WithTabIndex(string value) => WithAttribute(Attribute.TabIndex(value));
    }
}
