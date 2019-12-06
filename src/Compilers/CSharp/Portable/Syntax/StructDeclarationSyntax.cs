// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable enable

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax
{
    public partial class StructDeclarationSyntax
    {
        public StructDeclarationSyntax Update(
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxTokenList modifiers,
            SyntaxToken keyword,
            SyntaxToken identifier,
            TypeParameterListSyntax? typeParameterList,
            BaseListSyntax? baseList,
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses,
            SyntaxToken openBraceToken,
            SyntaxList<MemberDeclarationSyntax> members,
            SyntaxToken closeBraceToken,
            SyntaxToken semicolonToken)
        {
            if (attributeLists != this.AttributeLists || modifiers != this.Modifiers || keyword != this.Keyword || identifier != this.Identifier || typeParameterList != this.TypeParameterList || baseList != this.BaseList || constraintClauses != this.ConstraintClauses || openBraceToken != this.OpenBraceToken || members != this.Members || closeBraceToken != this.CloseBraceToken || semicolonToken != this.SemicolonToken)
            {
                var newNode = SyntaxFactory.StructDeclaration(attributeLists, modifiers, keyword, identifier, typeParameterList, parameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
                var annotations = GetAnnotations();
                return annotations?.Length > 0 ? newNode.WithAnnotations(annotations) : newNode;
            }

            return this;
        }
    }
}

namespace Microsoft.CodeAnalysis.CSharp
{
    public partial class SyntaxFactory
    {
        public static StructDeclarationSyntax StructDeclaration(
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxTokenList modifiers,
            SyntaxToken keyword,
            SyntaxToken identifier,
            TypeParameterListSyntax? typeParameterList,
            BaseListSyntax? baseList,
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses,
            SyntaxToken openBraceToken,
            SyntaxList<MemberDeclarationSyntax> members,
            SyntaxToken closeBraceToken,
            SyntaxToken semicolonToken)
            => SyntaxFactory.StructDeclaration(
                attributeLists,
                modifiers,
                keyword,
                identifier,
                typeParameterList,
                parameterList: null,
                baseList,
                constraintClauses,
                openBraceToken,
                members,
                closeBraceToken,
                semicolonToken);

        public static StructDeclarationSyntax StructDeclaration(
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxTokenList modifiers,
            SyntaxToken identifier,
            TypeParameterListSyntax typeParameterList,
            BaseListSyntax baseList,
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses,
            SyntaxList<MemberDeclarationSyntax> members)
            => StructDeclaration(
                attributeLists,
                modifiers,
                identifier,
                typeParameterList,
                parameterList: null,
                baseList,
                constraintClauses,
                members);
    }
}
