using System;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	public static class BasicStructures
	{
		// TODO: Define the break code by the file parsed
		internal static readonly RegexPattern Break = (RegexPattern) '\n';

		internal static readonly RegexPattern Spaces = Characters.Space.WithLimitingRepetition(min: 1);

		private static readonly RegexPattern _indent = Characters.Space.WithLimitingRepetition();

		private static readonly RegexPattern _anchoredIndent = _indent.WithAnchorAtBeginning();

		internal static readonly RegexPattern SeparateInLine =
			RegexPatternBuilder.BuildAlternation(
				RegexPattern.Empty.WithAnchorAtBeginning(),
				RegexPatternBuilder.BuildCharSet(Characters.Space, Characters.Tab)
					.WithLimitingRepetition(min: 1, asNonCapturingGroup: false)
			);

		private static readonly RegexPattern _tagHandle =
			Characters.Tag + Characters.WordChar.WithLimitingRepetition() + Characters.Tag.AsOptional();

		internal static RegexPattern LinePrefix(Context c, bool useAnchoredIndent = true)
		{
			var indent = useAnchoredIndent ? _anchoredIndent : _indent;

			return c switch
			{
				Context.BlockIn or Context.BlockOut => indent,
				Context.FlowIn or Context.FlowOut => indent + SeparateInLine.AsOptional(),
				_ => throw new ArgumentOutOfRangeException(nameof(c), c, $"Unknown {nameof(Context)} item {c}."),
			};
		}

		internal static RegexPattern EmptyLine(Context c, bool useAnchoredIndent = true)
		{
			return LinePrefix(c, useAnchoredIndent) + Break;
		}

		// TODO: Replace with CommentParser
		internal static readonly RegexPattern Comment =
			(
				SeparateInLine +
				(
					Characters.Comment +
					RegexPatternBuilder.BuildNegatedCharSet(Break)
						.WithLimitingRepetition(max: Characters.CommentTextMaxLength)
				).AsOptional()
			).AsOptional() + Break;

		internal static class Directives
		{
			public static RegexPattern YamlDirectiveName = (RegexPattern) "YAML";
			public static RegexPattern TagDirectiveName = (RegexPattern) "TAG";

			private static readonly RegexPattern _reservedDirectiveName =
				Characters.NsChar.AsNonCapturingGroup().WithLimitingRepetition(min: 1).AsCapturingGroup();

			private static readonly RegexPattern _parameter =
				Characters.NsChar.AsNonCapturingGroup().WithLimitingRepetition(min: 1).AsCapturingGroup();

			private static readonly RegexPattern _localTagPrefix =
				Characters.Tag + Characters.UriChar.WithLimitingRepetition();

			private static readonly string _globalTagPrefix =
				Characters.TagChar + Characters.UriChar.WithLimitingRepetition();

			private static readonly RegexPattern _tagPrefix = RegexPatternBuilder
				.BuildAlternation(_localTagPrefix, _globalTagPrefix).AsCapturingGroup();

			public static readonly RegexPattern Reserved =
				(
					Characters.Directive +
					_reservedDirectiveName +
					SeparateInLine +
					_parameter +
					Comment
				).WithAnchorAtBeginning();

			public static readonly RegexPattern Yaml =
				Characters.Directive.WithAnchorAtBeginning() +
				YamlDirectiveName +
				SeparateInLine +
				(
					RegexPatternBuilder.BuildCharSet(Characters.DecimalDigits).WithLimitingRepetition(min: 1) +
					Characters.Escape + Characters.VersionSeparator +
					RegexPatternBuilder.BuildCharSet(Characters.DecimalDigits).WithLimitingRepetition(min: 1)
				).AsCapturingGroup() +
				Comment;

			public static readonly string Tag =
				(
					Characters.Directive +
					 TagDirectiveName +
					 SeparateInLine +
					 _tagHandle.AsCapturingGroup() +
					 SeparateInLine +
					 _tagPrefix +
					 Comment
				).WithAnchorAtBeginning();
		}

		public static class NodeTags
		{
			// Even though '!<!>' satisfies the regex, it's not a valid verbatim tag since verbatim tags are not
			// subject to tag resolution.
			public static readonly RegexPattern VerbatimTag =
				appendLookAhead(new RegexPattern(
						$"!<{Characters.UriChar.WithLimitingRepetition(min: 1, asNonCapturingGroup: false)}>"
					)
					.AsCapturingGroup()
					.WithAnchorAtBeginning()
				);

			public static readonly RegexPattern ShorthandTag =
				appendLookAhead(
					(_tagHandle + Characters.TagChar.WithLimitingRepetition(min: 1))
						.AsCapturingGroup()
						.WithAnchorAtBeginning()
				);

			public static readonly RegexPattern NonSpecificTag = appendLookAhead(Characters.Tag.AsCapturingGroup());

			public static readonly RegexPattern AnchorName =
				Characters.AnchorChar.WithLimitingRepetition(min: 1, asNonCapturingGroup: false).AsCapturingGroup();

			public static readonly RegexPattern AnchorProperty = appendLookAhead(Characters.Anchor + AnchorName);

			private static RegexPattern appendLookAhead(RegexPattern pattern) =>
				RegexPatternBuilder.BuildLookAhead(
					beforeChars: pattern,
					lookAheadExpression: RegexPatternBuilder.BuildCharSet(Characters.SWhites + Break)
				);
		}
	}
}
