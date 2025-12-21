using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Serializator
{

    [Generator]
    public sealed class BinarySerializerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new GenerateSerializerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxReceiver as GenerateSerializerSyntaxReceiver;
            if (receiver == null)
                return;

            foreach (var classDecl in receiver.Candidates)
            {
                var semanticModel = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (classSymbol == null)
                    continue;

                if (!HasGenerateSerializerAttribute(classSymbol))
                    continue;

                string source = GenerateSerializerForClass(classSymbol);
                if (!string.IsNullOrWhiteSpace(source))
                {
                    context.AddSource(
                        $"{classSymbol.Name}_BinarySerializer.g.cs",
                        SourceText.From(source, Encoding.UTF8));
                }
            }
        }

        private static bool HasGenerateSerializerAttribute(INamedTypeSymbol classSymbol)
        {
            foreach (var attr in classSymbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;
                if (attrClass == null)
                    continue;

                var name = attrClass.Name;
                var fullName = attrClass.ToDisplayString();

                if (name == "GenerateBinarySerializerAttribute" ||
                    fullName == "Serializator.GenerateBinarySerializerAttribute")
                {
                    return true;
                }
            }

            return false;
        }

        private static string GenerateSerializerForClass(INamedTypeSymbol classSymbol)
        {
            IPropertySymbol[] properties = classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
                .ToArray();

            if (properties.Length == 0)
                return string.Empty;

            string ns = classSymbol.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : classSymbol.ContainingNamespace.ToDisplayString();

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(ns))
            {
                sb.Append("namespace ").Append(ns).AppendLine(";");
                sb.AppendLine();
            }

            sb.Append("public partial class ").Append(classSymbol.Name).AppendLine();
            sb.AppendLine("{");

            AppendSerializeMethod(sb, classSymbol.Name, properties);

            sb.AppendLine();

            AppendDeserializeMethod(sb, classSymbol.Name, properties);

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AppendSerializeMethod(StringBuilder sb, string typeName, IPropertySymbol[] properties)
        {
            sb.AppendLine("    public void SerializeToBinary(Stream stream)");
            sb.AppendLine("    {");
            sb.AppendLine("        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);");

            foreach (var prop in properties)
            {
                AppendWriteProperty(sb, prop);
            }

            sb.AppendLine("    }");
        }

        private static void AppendWriteProperty(StringBuilder sb, IPropertySymbol prop)
        {
            if (prop.Type.SpecialType == SpecialType.System_Int32)
            {
                sb.Append("        writer.Write(this.")
                  .Append(prop.Name)
                  .AppendLine(");");
            }
            else if (prop.Type.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine("        if (this." + prop.Name + " == null)");
                sb.AppendLine("        {");
                sb.AppendLine("            writer.Write(-1);");
                sb.AppendLine("        }");
                sb.AppendLine("        else");
                sb.AppendLine("        {");
                sb.AppendLine("            var bytes = System.Text.Encoding.UTF8.GetBytes(this." + prop.Name + ");");
                sb.AppendLine("            writer.Write(bytes.Length);");
                sb.AppendLine("            writer.Write(bytes);");
                sb.AppendLine("        }");
            }
            else if (prop.Type.SpecialType == SpecialType.System_Boolean)
            {
                sb.Append("        writer.Write(this.")
                  .Append(prop.Name)
                  .AppendLine(");");
            }
            else if (prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                     == "global::System.DateTime")
            {
                sb.Append($"        writer.Write(this.")
                  .Append(prop.Name)
                  .Append(".Ticks);")
                  .AppendLine();
            }
            else
            {
                sb.Append("        // Unsupported type: ")
                  .Append(prop.Type.ToDisplayString())
                  .Append("of property ")
                  .Append(prop.Name)
                  .AppendLine();
            }
        }

        private static void AppendDeserializeMethod(StringBuilder sb, string typeName, IPropertySymbol[] properties)
        {
            sb.AppendLine($"    public static {typeName} DeserializeFromBinary(Stream stream)");
            sb.AppendLine("    {");
            sb.AppendLine("        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);");
            sb.AppendLine($"        var obj = new {typeName}();");

            foreach (var prop in properties)
            {
                AppendReadProperty(sb, prop);
            }

            sb.AppendLine("        return obj;");
            sb.AppendLine("    }");
        }

        private static void AppendReadProperty(StringBuilder sb, IPropertySymbol prop)
        {
            if (prop.Type.SpecialType == SpecialType.System_Int32)
            {
                sb.Append("        obj.").Append(prop.Name).Append(" = reader.ReadInt32();").AppendLine();
            }
            else if (prop.Type.SpecialType == SpecialType.System_Boolean)
            {
                sb.Append("        obj.").Append(prop.Name).Append(" = reader.ReadBoolean();").AppendLine();
            }
            else if (prop.Type.SpecialType == SpecialType.System_String)
            {
                sb.Append("        obj.").Append(prop.Name).Append(" = reader.ReadString();").AppendLine();
            }
            else if (prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                     == "global::System.DateTime")
            {
                sb.Append($"        obj.").Append(prop.Name).Append(" = new DateTime(reader.ReadInt64());").AppendLine();
            }
            else
            {
                sb.Append("        // Unsupported type: ")
                  .Append(prop.Type.ToDisplayString())
                  .Append("of property ")
                  .Append(prop.Name)
                  .AppendLine();
            }
        }
    }
}