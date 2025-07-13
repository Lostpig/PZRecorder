using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace PZPKRecorderGenerator;

[Generator(LanguageNames.CSharp)]
public class LocalizationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Debugger.Launch();
        IncrementalValuesProvider<AdditionalText> textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith("languages.json"));

        context.RegisterSourceOutput(textFiles, (spc, tfs) =>
        {
            if (Path.GetFileNameWithoutExtension(tfs.Path) != "languages") return;

            var jsonText = tfs.GetText()?.ToString();
            if (jsonText == null)
            {
                throw new Exception("cannot read languages.json file.");
            }

            var sourceText = GetLocalizeDictSource(jsonText);
            spc.AddSource("LocalizeDict.g.cs", sourceText);
        });
    }

    private static SourceText GetLocalizeDictSource(string languageJsonText)
    {
        var languagesJson = Helpers.LocalizationHelper.DeserializeLanguage(languageJsonText);

        var sourceText = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("PZPKRecorder.Localization"))
            .AddMembers(
                SyntaxFactory.ClassDeclaration("LocalizeDict")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .AddMembers(GetFieldProterties(languagesJson))
                    .AddMembers(GetUpdateMethod(languagesJson))
            )
            .NormalizeWhitespace()
            .GetText(Encoding.UTF8);
        return sourceText;
    }
    private static MemberDeclarationSyntax[] GetFieldProterties(Helpers.LanguageJson language)
    {
        return language.Fields.Select(field =>
            SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), field)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List<AccessorDeclarationSyntax>(
                            new[]
                            {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }
                        )
                    )
                )
        ).ToArray();
    }
    private static MemberDeclarationSyntax GetUpdateMethod(Helpers.LanguageJson language)
    {
        var fields = language.Fields.Select(f => $"{f} = getText(\"{f}\");");

        var code = $@"
            public static void Update(Dictionary<string, string> dict)
            {{
                {string.Join("\r\n", fields)}
                
                string getText(string key)
                {{
                    if (dict != null && dict.ContainsKey(key))
                    {{
                        return dict[key];
                    }}
                    return key;
                }}
            }}
        ";

        return SyntaxFactory.ParseMemberDeclaration(code);
    }
}
