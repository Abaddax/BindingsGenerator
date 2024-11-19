using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using CppSharp.AST;
using System.Text.RegularExpressions;

namespace BindingsGenerator.Generator.Unsafe.Internal.Models.Processor
{
    internal static class ObsoletionHelper
    {
        private static string GetMessage(Declaration declaration)
        {
            var lines = declaration.Comment?.FullComment?.Blocks
                .OfType<BlockCommandComment>()
                .Where(x => x.CommandKind == CommentCommandKind.Deprecated)
                .SelectMany(x =>
                    x.ParagraphComment.Content.OfType<TextComment>().Select(c => c.Text.Trim())
                );
            var obsoleteMessage = lines == null ? string.Empty : string.Join(" ", lines);
            return obsoleteMessage;
        }
        private static string? GetCommentString(BlockCommandComment? comment)
        {
            return comment == null
                ? null
                : string.Join(" ", comment.ParagraphComment.Content.OfType<TextComment>().Select(x => x.Text.Trim()));
        }

        public static Obsoletion GetObsoletion(this Declaration declaration)
        {
            var message = GetMessage(declaration);
            return new Obsoletion
            {
                IsObsolete = declaration.IsDeprecated || !string.IsNullOrWhiteSpace(message),
                Message = message
            };
        }
        public static string? GetDocumentation(this Declaration declaration)
        {
            return declaration?.Comment?.BriefText ?? string.Empty;
        }
        public static string? GetReturnDocumentation(this Function function)
        {
            var comment = function?.Comment?.FullComment.Blocks
                .OfType<BlockCommandComment>()
                .FirstOrDefault(x => x.CommandKind == CommentCommandKind.Return);
            return GetCommentString(comment);
        }
        public static string? GetParamDocumentation(this Function function, string parameterName)
        {
            var comment = function?.Comment?.FullComment.Blocks
                .OfType<ParamCommandComment>()
                .FirstOrDefault(x => x.Arguments.Count == 1 && x.Arguments[0].Text == parameterName);
            return GetCommentString(comment);
        }

        public static string GetName(this Declaration declaration, Declaration? parent)
        {
            if (!string.IsNullOrEmpty(declaration.Name))
                return declaration.Name;
            if (!string.IsNullOrEmpty(parent?.Name))
                return $"anonymousType_{parent.Name}_{declaration.LineNumberStart}";
            if (declaration?.TranslationUnit?.FileNameWithoutExtension == null)
                throw new Exception("Unable to resolve name for anonymous type without parent");
            return $"anonymousType_{declaration.TranslationUnit.FileNameWithoutExtension.RemoveInvalidCharacters()}_{declaration.LineNumberStart}";
        }

        public static string RemoveInvalidCharacters(this string str)
        {
            return Regex.Replace(str, @"[^a-zA-Z0-9_]", "");
        }
    }
}
