' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.Emit
Imports Microsoft.CodeAnalysis.VisualBasic.Emit

Namespace Microsoft.CodeAnalysis.VisualBasic.Symbols

    Friend Partial Class VisualBasicCustomModifier

        Friend Overrides Function GetModifier(context As EmitContext) As Cci.ITypeReference
            Return DirectCast(context.Module, PEModuleBuilder).Translate(Me.ModifierSymbol, DirectCast(context.SyntaxNodeOpt, VisualBasicSyntaxNode), context.Diagnostics)
        End Function
    End Class
End Namespace
