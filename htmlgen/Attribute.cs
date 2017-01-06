namespace HtmlGenerator 
{
    public static class Attribute 
    {
		
		public static HtmlNameAttribute Name(string value) => new HtmlNameAttribute(value);
		public static HtmlNoValidateAttribute NoValidate => new HtmlNoValidateAttribute();
		public static HtmlNoWrapAttribute NoWrap(string value) => new HtmlNoWrapAttribute(value);

		public static HtmlOptimumAttribute Optimum(string value) => new HtmlOptimumAttribute(value);
		public static HtmlOpenAttribute Open(string value) => new HtmlOpenAttribute(value);

		public static HtmlPatternAttribute Pattern(string value) => new HtmlPatternAttribute(value);
		public static HtmlPingAttribute Ping(string value) => new HtmlPingAttribute(value);
		public static HtmlPlaceholderAttribute Placeholder(string value) => new HtmlPlaceholderAttribute(value);
		public static HtmlPreloadAttribute Preload(string value) => new HtmlPreloadAttribute(value);
		public static HtmlPosterAttribute Poster(string value) => new HtmlPosterAttribute(value);

		public static HtmlRadioGroupAttribute RadioGroup(string value) => new HtmlRadioGroupAttribute(value);
		public static HtmlReadonlyAttribute Readonly => new HtmlReadonlyAttribute();
		public static HtmlRefreshAttribute Refresh(string value) => new HtmlRefreshAttribute(value);
		public static HtmlRelAttribute Rel(string value) => new HtmlRelAttribute(value);
		public static HtmlRequiredAttribute Required => new HtmlRequiredAttribute();
		public static HtmlReversedAttribute Reversed(string value) => new HtmlReversedAttribute(value);
		public static HtmlRowsAttribute Rows(string value) => new HtmlRowsAttribute(value);
		public static HtmlRowSpanAttribute RowSpan(string value) => new HtmlRowSpanAttribute(value);

		public static HtmlSandboxAttribute Sandbox => new HtmlSandboxAttribute();
		public static HtmlSeamlessAttribute Seamless => new HtmlSeamlessAttribute();
		public static HtmlSelectedAttribute Selected => new HtmlSelectedAttribute();
		public static HtmlSelectionDirectionAttribute SelectionDirection(string value) => new HtmlSelectionDirectionAttribute(value);
		public static HtmlScopeAttribute Scope(string value) => new HtmlScopeAttribute(value);
		public static HtmlScopedAttribute Scoped => new HtmlScopedAttribute();
		public static HtmlShapeAttribute Shape(string value) => new HtmlShapeAttribute(value);
		public static HtmlSizeAttribute Size(string value) => new HtmlSizeAttribute(value);
		public static HtmlSizesAttribute Sizes(string value) => new HtmlSizesAttribute(value);
		public static HtmlSpanAttribute Span(string value) => new HtmlSpanAttribute(value);
		public static HtmlSpellCheckAttribute SpellCheck(string value) => new HtmlSpellCheckAttribute(value);
		public static HtmlSrcAttribute Src(string value) => new HtmlSrcAttribute(value);
		public static HtmlSrcDocAttribute SrcDoc(string value) => new HtmlSrcDocAttribute(value);
		public static HtmlSrcLangAttribute SrcLang(string value) => new HtmlSrcLangAttribute(value);
		public static HtmlSrcSetAttribute SrcSet(string value) => new HtmlSrcSetAttribute(value);
		public static HtmlStartAttribute Start(string value) => new HtmlStartAttribute(value);
		public static HtmlStepAttribute Step(string value) => new HtmlStepAttribute(value);
		public static HtmlStyleAttribute Style(string value) => new HtmlStyleAttribute(value);

		public static HtmlTabIndexAttribute TabIndex(string value) => new HtmlTabIndexAttribute(value);
		public static HtmlTargetAttribute Target(string value) => new HtmlTargetAttribute(value);
		public static HtmlTextAttribute Text(string value) => new HtmlTextAttribute(value);
		public static HtmlTitleAttribute Title(string value) => new HtmlTitleAttribute(value);
		public static HtmlTranslateAttribute Translate(string value) => new HtmlTranslateAttribute(value);
		public static HtmlTypeAttribute Type(string value) => new HtmlTypeAttribute(value);
		public static HtmlTypeMustMatchAttribute TypeMustMatch => new HtmlTypeMustMatchAttribute();

		public static HtmlUseMapAttribute UseMap(string value) => new HtmlUseMapAttribute(value);

		public static HtmlValueAttribute Value(string value) => new HtmlValueAttribute(value);
		public static HtmlVolumeAttribute Volume(string value) => new HtmlVolumeAttribute(value);

		public static HtmlWidthAttribute Width(string value) => new HtmlWidthAttribute(value);
		public static HtmlWrapAttribute Wrap(string value) => new HtmlWrapAttribute(value);

		public static HtmlXmlsAttribute Xmls(string value) => new HtmlXmlsAttribute(value);
    }
}