﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class AttributeTests_Nullable : CSharpTestBase
    {
        // An empty project should not require System.Attribute.
        [Fact]
        public void EmptyProject_MissingAttribute()
        {
            var source = "";
            var comp = CreateEmptyCompilation(source, parseOptions: TestOptions.Regular8);
            // PROTOTYPE(NullableReferenceTypes): Do not emit [module: NullableAttribute] if no nullable types,
            // or change the missing System.Attribute error to a warning in this case.
            comp.VerifyEmitDiagnostics(
                // warning CS8021: No value for RuntimeMetadataVersion found. No assembly containing System.Object was found nor was a value for RuntimeMetadataVersion specified through options.
                Diagnostic(ErrorCode.WRN_NoRuntimeMetadataVersion).WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Boolean' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Boolean").WithLocation(1, 1));
        }

        [Fact]
        public void ExplicitAttributeFromSource()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute() { }
        public NullableAttribute(bool[] b) { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ExplicitAttributeFromMetadata()
        {
            var source0 =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute() { }
        public NullableAttribute(bool[] b) { }
    }
}";
            var comp0 = CreateCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(source, references: new[] { ref0 }, parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ExplicitAttribute_MissingParameterlessConstructor()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(bool[] b) { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (10,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 19),
                // (10,30): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?[] y").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 30));
        }

        // PROTOTYPE(NullableReferenceTypes): Handle missing constructor.
        [Fact(Skip = "Handle missing constructor")]
        public void ExplicitAttribute_MissingConstructor()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute() { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (10,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 19),
                // (10,30): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?[] y").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 30));
        }

        [Fact]
        public void NullableAttribute_MissingBoolean()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public class Attribute
    {
    }
}";
            var comp0 = CreateEmptyCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // error CS0518: Predefined type 'System.Boolean' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Boolean").WithLocation(1, 1));
        }

        [Fact]
        public void NullableAttribute_MissingAttribute()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
}";
            var comp0 = CreateEmptyCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1));
        }

        [Fact]
        public void NullableAttribute_StaticAttributeConstructorOnly()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public class Attribute
    {
        static Attribute() { }
        public Attribute(object o) { }
    }
}";
            var comp0 = CreateEmptyCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // error CS1729: 'Attribute' does not contain a constructor that takes 0 arguments
                Diagnostic(ErrorCode.ERR_BadCtorArgCount).WithArguments("System.Attribute", "0").WithLocation(1, 1),
                // error CS1729: 'Attribute' does not contain a constructor that takes 0 arguments
                Diagnostic(ErrorCode.ERR_BadCtorArgCount).WithArguments("System.Attribute", "0").WithLocation(1, 1),
                // error CS1729: 'Attribute' does not contain a constructor that takes 0 arguments
                Diagnostic(ErrorCode.ERR_BadCtorArgCount).WithArguments("System.Attribute", "0").WithLocation(1, 1));
        }

        [Fact]
        public void EmitAttribute_NoNullable()
        {
            var source =
@"public class C
{
    public object F = new object();
}";
            // C# 7.0: No NullableAttribute.
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular7);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                var type = assembly.GetTypeByMetadataName("C");
                var field = (FieldSymbol)type.GetMembers("F").Single();
                AssertNoNullableAttribute(field.GetAttributes());
                AssertNoNullableAttribute(module.GetAttributes());
                AssertAttributes(assembly.GetAttributes(),
                    "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                    "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                    "System.Diagnostics.DebuggableAttribute");
            });
            // C# 8.0: NullableAttribute included always.
            comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                var type = assembly.GetTypeByMetadataName("C");
                var field = (FieldSymbol)type.GetMembers("F").Single();
                AssertNoNullableAttribute(field.GetAttributes());
                AssertNullableAttribute(module.GetAttributes());
                AssertAttributes(assembly.GetAttributes(),
                    "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                    "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                    "System.Diagnostics.DebuggableAttribute");
            });
        }

        [Fact]
        public void EmitAttribute_Module()
        {
            var source =
@"public class C
{
    public object? F = new object();
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                var type = assembly.GetTypeByMetadataName("C");
                var field = (FieldSymbol)type.GetMembers("F").Single();
                AssertNullableAttribute(field.GetAttributes());
                AssertNullableAttribute(module.GetAttributes());
                AssertAttributes(assembly.GetAttributes(),
                    "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                    "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                    "System.Diagnostics.DebuggableAttribute");
            });
        }

        [Fact]
        public void EmitAttribute_NetModule()
        {
            var source =
@"public class C
{
    public object? F = new object();
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,20): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public object? F = new object();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "F").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 20));
        }

        [Fact]
        public void EmitAttribute_NetModuleNoDeclarations()
        {
            var source = "";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            CompileAndVerify(comp, verify: Verification.Skipped, symbolValidator: module =>
            {
                AssertAttributes(module.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_BaseClass()
        {
            var source =
@"public class A<T>
{
}
public class B1 : A<object>
{
}
public class B2 : A<object?>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("A`1");
                AssertNoNullableAttribute(type.GetAttributes());
                type = module.ContainingAssembly.GetTypeByMetadataName("B1");
                AssertNoNullableAttribute(type.BaseType().GetAttributes());
                AssertNoNullableAttribute(type.GetAttributes());
                type = module.ContainingAssembly.GetTypeByMetadataName("B2");
                AssertNoNullableAttribute(type.BaseType().GetAttributes());
                AssertNullableAttribute(type.GetAttributes());
            });
            var source2 =
@"class C
{
    static void F(A<object> x, A<object?> y)
    {
    }
    static void G(B1 x, B2 y)
    {
        F(x, x);
        F(y, y);
    }
}";
            var comp2 = CreateCompilation(source2, parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (8,14): warning CS8620: Nullability of reference types in argument of type 'B1' doesn't match target type 'A<object?>' for parameter 'y' in 'void C.F(A<object> x, A<object?> y)'.
                //         F(x, x);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "x").WithArguments("B1", "A<object?>", "y", "void C.F(A<object> x, A<object?> y)").WithLocation(8, 14),
                // (9,11): warning CS8620: Nullability of reference types in argument of type 'B2' doesn't match target type 'A<object>' for parameter 'x' in 'void C.F(A<object> x, A<object?> y)'.
                //         F(y, y);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "y").WithArguments("B2", "A<object>", "x", "void C.F(A<object> x, A<object?> y)").WithLocation(9, 11));
        }

        [Fact]
        public void EmitAttribute_Interface_01()
        {
            var source =
@"public interface I<T>
{
}
public class A : I<object>
{
}
public class B : I<object?>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "A");
                var interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes());
                typeDef = GetTypeDefinitionByName(reader, "B");
                interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
            });
            var source2 =
@"class C
{
    static void F(I<object> x, I<object?> y)
    {
    }
    static void G(A x, B y)
    {
        F(x, x);
        F(y, y);
    }
}";
            var comp2 = CreateCompilation(source2, parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (8,14): warning CS8620: Nullability of reference types in argument of type 'A' doesn't match target type 'I<object?>' for parameter 'y' in 'void C.F(I<object> x, I<object?> y)'.
                //         F(x, x);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "x").WithArguments("A", "I<object?>", "y", "void C.F(I<object> x, I<object?> y)").WithLocation(8, 14),
                // (9,11): warning CS8620: Nullability of reference types in argument of type 'B' doesn't match target type 'I<object>' for parameter 'x' in 'void C.F(I<object> x, I<object?> y)'.
                //         F(y, y);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "y").WithArguments("B", "I<object>", "x", "void C.F(I<object> x, I<object?> y)").WithLocation(9, 11));
        }

        [Fact]
        public void EmitAttribute_Interface_02()
        {
            var source =
@"public interface I<T>
{
}
public class A : I<(object X, object Y)>
{
}
public class B : I<(object X, object? Y)>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "A");
                var interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(),
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])");
                typeDef = GetTypeDefinitionByName(reader, "B");
                interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(),
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                    "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
            });

            var source2 =
@"class C
{
    static void F(I<(object, object)> a, I<(object, object?)> b)
    {
    }
    static void G(A a, B b)
    {
        F(a, a);
        F(b, b);
    }
}";
            var comp2 = CreateCompilation(source2, parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (8,14): warning CS8620: Nullability of reference types in argument of type 'A' doesn't match target type 'I<(object, object?)>' for parameter 'b' in 'void C.F(I<(object, object)> a, I<(object, object?)> b)'.
                //         F(a, a);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "a").WithArguments("A", "I<(object, object?)>", "b", "void C.F(I<(object, object)> a, I<(object, object?)> b)").WithLocation(8, 14),
                // (9,11): warning CS8620: Nullability of reference types in argument of type 'B' doesn't match target type 'I<(object, object)>' for parameter 'a' in 'void C.F(I<(object, object)> a, I<(object, object?)> b)'.
                //         F(b, b);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "b").WithArguments("B", "I<(object, object)>", "a", "void C.F(I<(object, object)> a, I<(object, object?)> b)").WithLocation(9, 11));

            var type = comp2.GetMember<NamedTypeSymbol>("A");
            Assert.Equal("I<(System.Object X, System.Object Y)>", type.Interfaces()[0].ToTestDisplayString());
            type = comp2.GetMember<NamedTypeSymbol>("B");
            Assert.Equal("I<(System.Object X, System.Object? Y)>", type.Interfaces()[0].ToTestDisplayString());
        }

        // PROTOTYPE(NullableReferenceTypes): Execute some of these same tests with feature disabled.

        [Fact]
        public void EmitAttribute_Constraint()
        {
            var source =
@"public class A<T>
{
}
public class B<T> where T : A<object?>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "B`1");
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
            });

            var source2 =
@"class Program
{
    static void Main()
    {
        new B<A<object?>>();
        new B<A<object>>();
    }
}";
            var comp2 = CreateCompilation(source2, parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            // PROTOTYPE(NullableReferenceTypes): Report warning for `new B<A<object>>()`:
            // https://github.com/dotnet/roslyn/issues/27742.
            comp2.VerifyDiagnostics();

            var type = comp2.GetMember<NamedTypeSymbol>("B");
            Assert.Equal("A<System.Object?>", type.TypeParameters[0].ConstraintTypes()[0].ToTestDisplayString());
        }

        [Fact]
        public void EmitAttribute_MethodReturnType()
        {
            var source =
@"public class C
{
    public object? F() => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                AssertNullableAttribute(method.GetReturnTypeAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_MethodParameters()
        {
            var source =
@"public class C
{
    public void F(object?[] c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                AssertNullableAttribute(method.Parameters.Single().GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_ConstructorParameters()
        {
            var source =
@"public class C
{
    public C(object?[] c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = type.Constructors.Single();
                AssertNullableAttribute(method.Parameters.Single().GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_PropertyType()
        {
            var source =
@"public class C
{
    public object? P => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var property = (PropertySymbol)type.GetMembers("P").Single();
                AssertNullableAttribute(property.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_PropertyParameters()
        {
            var source =
@"public class C
{
    public object this[object x, object? y] => throw new System.NotImplementedException();
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var property = (PropertySymbol)type.GetMembers("this[]").Single();
                AssertNoNullableAttribute(property.Parameters[0].GetAttributes());
                AssertNullableAttribute(property.Parameters[1].GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_OperatorReturnType()
        {
            var source =
@"public class C
{
    public static object? operator+(C a, C b) => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("op_Addition").Single();
                AssertNullableAttribute(method.GetReturnTypeAttributes());
                AssertNoNullableAttribute(method.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_OperatorParameters()
        {
            var source =
@"public class C
{
    public static object operator+(C a, object?[] b) => a;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("op_Addition").Single();
                AssertNoNullableAttribute(method.Parameters[0].GetAttributes());
                AssertNullableAttribute(method.Parameters[1].GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_DelegateReturnType()
        {
            var source =
@"public delegate object? D();";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var method = module.ContainingAssembly.GetTypeByMetadataName("D").DelegateInvokeMethod;
                AssertNullableAttribute(method.GetReturnTypeAttributes());
                AssertNoNullableAttribute(method.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_DelegateParameters()
        {
            var source =
@"public delegate void D(object?[] o);";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var method = module.ContainingAssembly.GetTypeByMetadataName("D").DelegateInvokeMethod;
                AssertNullableAttribute(method.Parameters[0].GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_LambdaReturnType()
        {
            var source =
@"delegate T D<T>();
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G(object o)
    {
        F(() =>
        {
            if (o != new object()) return o;
            return null;
        });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            // PROTOTYPE(NullableReferenceTypes): Use AssertNullableAttribute(method.GetReturnTypeAttributes()). 
            AssertAttribute(comp);
        }

        [Fact]
        public void EmitAttribute_LambdaParameters()
        {
            var source =
@"delegate void D<T>(T t);
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G()
    {
        F((object? o) => { });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            // PROTOTYPE(NullableReferenceTypes): Use AssertNullableAttribute(method.Parameters[0].GetAttributes()). 
            AssertAttribute(comp);
        }

        [Fact]
        public void EmitAttribute_LocalFunctionReturnType()
        {
            var source =
@"class C
{
    static void M()
    {
        object?[] L() => throw new System.NotImplementedException();
        L();
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var method = module.ContainingAssembly.GetTypeByMetadataName("C").GetMethod("<M>g__L|0_0");
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertAttributes(method.GetAttributes(), "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                });
        }

        [Fact]
        public void EmitAttribute_LocalFunctionParameters()
        {
            var source =
@"class C
{
    static void M()
    {
        void L(object? x, object y) { }
        L(null, 2);
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var method = module.ContainingAssembly.GetTypeByMetadataName("C").GetMethod("<M>g__L|0_0");
                    AssertNullableAttribute(method.Parameters[0].GetAttributes());
                    AssertNoNullableAttribute(method.Parameters[1].GetAttributes());
                });
        }

        [Fact]
        public void EmitAttribute_ExplicitImplementationForwardingMethod()
        {
            var source0 =
@"public class A
{
    public object? F() => null;
}";
            var comp0 = CreateCompilation(source0, parseOptions: TestOptions.Regular8);
            var ref0 = comp0.EmitToImageReference();
            var source =
@"interface I
{
    object? F();
}
class B : A, I
{
}";
            CompileAndVerify(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var method = module.ContainingAssembly.GetTypeByMetadataName("B").GetMethod("I.F");
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertNoNullableAttribute(method.GetAttributes());
                });
        }

        [Fact]
        public void EmitAttribute_Iterator_01()
        {
            var source =
@"using System.Collections.Generic;
class C
{
    static IEnumerable<object?> F()
    {
        yield break;
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var property = module.ContainingAssembly.GetTypeByMetadataName("C").GetTypeMember("<F>d__0").GetProperty("System.Collections.Generic.IEnumerator<System.Object>.Current");
                    AssertNoNullableAttribute(property.GetAttributes());
                    var method = property.GetMethod;
                    // PROTOTYPE(NullableReferenceTypes): No synthesized attributes for this
                    // case which is inconsisten with IEnumerable<object?[]> in test below.
                    AssertNoNullableAttribute(method.GetReturnTypeAttributes());
                    AssertAttributes(method.GetAttributes(), "System.Diagnostics.DebuggerHiddenAttribute");
                });
        }

        [Fact]
        public void EmitAttribute_Iterator_02()
        {
            var source =
@"using System.Collections.Generic;
class C
{
    static IEnumerable<object?[]> F()
    {
        yield break;
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var property = module.ContainingAssembly.GetTypeByMetadataName("C").GetTypeMember("<F>d__0").GetProperty("System.Collections.Generic.IEnumerator<System.Object[]>.Current");
                    AssertNoNullableAttribute(property.GetAttributes());
                    var method = property.GetMethod;
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertAttributes(method.GetAttributes(), "System.Diagnostics.DebuggerHiddenAttribute");
                });
        }

        [Fact]
        public void UseSiteError_LambdaReturnType()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct IntPtr { }
    public class MulticastDelegate { }
}";
            var comp0 = CreateEmptyCompilation(source0);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"delegate T D<T>();
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G(object o)
    {
        F(() =>
        {
            if (o != new object()) return o;
            return null;
        });
    }
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1));
        }

        [Fact]
        public void ModuleMissingAttribute_BaseClass()
        {
            var source =
@"class A<T>
{
}
class B : A<object?>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (4,7): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // class B : A<object?>
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "B").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(4, 7));
        }

        [Fact]
        public void ModuleMissingAttribute_Interface()
        {
            var source =
@"interface I<T>
{
}
class C : I<(object X, object? Y)>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (4,7): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // class C : I<(object X, object? Y)>
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "C").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(4, 7));
        }

        [Fact]
        public void ModuleMissingAttribute_MethodReturnType()
        {
            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,5): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object? F() => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 5));
        }

        [Fact]
        public void ModuleMissingAttribute_MethodParameters()
        {
            var source =
@"class C
{
    void F(object?[] c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,12): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     void F(object?[] c) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] c").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 12));
        }

        [Fact]
        public void ModuleMissingAttribute_ConstructorParameters()
        {
            var source =
@"class C
{
    C(object?[] c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,7): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     C(object?[] c) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] c").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 7));
        }

        [Fact]
        public void ModuleMissingAttribute_PropertyType()
        {
            var source =
@"class C
{
    object? P => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,5): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object? P => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 5));
        }

        [Fact]
        public void ModuleMissingAttribute_PropertyParameters()
        {
            var source =
@"class C
{
    object this[object x, object? y] => throw new System.NotImplementedException();
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,27): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object this[object x, object? y] => throw new System.NotImplementedException();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object? y").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 27));
        }

        [Fact]
        public void ModuleMissingAttribute_OperatorReturnType()
        {
            var source =
@"class C
{
    public static object? operator+(C a, C b) => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,19): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object? operator+(C a, C b) => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 19));
        }

        [Fact]
        public void ModuleMissingAttribute_OperatorParameters()
        {
            var source =
@"class C
{
    public static object operator+(C a, object?[] b) => a;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (3,41): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object operator+(C a, object?[] b) => a;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] b").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 41));
        }

        [Fact]
        public void ModuleMissingAttribute_DelegateReturnType()
        {
            var source =
@"delegate object? D();";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (1,10): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // delegate object? D();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 10));
        }

        [Fact]
        public void ModuleMissingAttribute_DelegateParameters()
        {
            var source =
@"delegate void D(object?[] o);";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (1,17): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // delegate void D(object?[] o);
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] o").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 17));
        }

        [Fact]
        public void ModuleMissingAttribute_LambdaReturnType()
        {
            var source =
@"delegate T D<T>();
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G(object o)
    {
        F(() =>
        {
            if (o != new object()) return o;
            return null;
        });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            // The lambda signature is emitted without a [Nullable] attribute because
            // the return type is inferred from flow analysis, not from initial binding.
            // As a result, there is no missing attribute warning.
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ModuleMissingAttribute_LambdaParameters()
        {
            var source =
@"delegate void D<T>(T t);
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G()
    {
        F((object? o) => { });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (9,12): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         F((object? o) => { });
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object? o").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(9, 12));
        }

        [Fact]
        public void ModuleMissingAttribute_LocalFunctionReturnType()
        {
            var source =
@"class C
{
    static void M()
    {
        object?[] L() => throw new System.NotImplementedException();
        L();
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (5,9): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         object?[] L() => throw new System.NotImplementedException();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[]").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(5, 9));
        }

        [Fact]
        public void ModuleMissingAttribute_LocalFunctionParameters()
        {
            var source =
@"class C
{
    static void M()
    {
        void L(object? x, object y) { }
        L(null, 2);
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            comp.VerifyEmitDiagnostics(
                // (5,16): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         void L(object? x, object y) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(5, 16));
        }

        [Fact]
        public void Tuples()
        {
            var source =
@"public class A
{
    public static ((object?, object) _1, object? _2, object _3, ((object?[], object), object?) _4) Nested;
    public static (object? _1, object _2, object? _3, object _4, object? _5, object _6, object? _7, object _8, object? _9) Long;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "A");
                var fieldDefs = typeDef.GetFields().Select(f => reader.GetFieldDefinition(f)).ToArray();

                // Nested tuple
                var field = fieldDefs.Single(f => reader.StringComparer.Equals(f.Name, "Nested"));
                var customAttributes = field.GetCustomAttributes();
                AssertAttributes(reader, customAttributes,
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                    "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
                var customAttribute = reader.GetCustomAttribute(customAttributes.ElementAt(1));
                AssertEx.Equal(ImmutableArray.Create(false, false, true, false, true, false, false, false, false, true, false, true), reader.ReadBoolArray(customAttribute.Value));

                // Long tuple
                field = fieldDefs.Single(f => reader.StringComparer.Equals(f.Name, "Long"));
                customAttributes = field.GetCustomAttributes();
                AssertAttributes(reader, customAttributes,
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                    "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
                customAttribute = reader.GetCustomAttribute(customAttributes.ElementAt(1)); 
                AssertEx.Equal(ImmutableArray.Create(false, true, false, true, false, true, false, true, false, false, true), reader.ReadBoolArray(customAttribute.Value));
            });

            var source2 =
@"class B
{
    static void Main()
    {
        A.Nested._1.Item1.ToString(); // 1
        A.Nested._1.Item2.ToString();
        A.Nested._2.ToString(); // 2
        A.Nested._3.ToString();
        A.Nested._4.Item1.Item1.ToString();
        A.Nested._4.Item1.Item1[0].ToString(); // 3
        A.Nested._4.Item1.Item2.ToString();
        A.Nested._4.Item2.ToString(); // 4
        A.Long._1.ToString(); // 5
        A.Long._2.ToString();
        A.Long._3.ToString(); // 6
        A.Long._4.ToString();
        A.Long._5.ToString(); // 7
        A.Long._6.ToString();
        A.Long._7.ToString(); // 8
        A.Long._8.ToString();
        A.Long._9.ToString(); // 9
    }
}";
            var comp2 = CreateCompilation(source2, parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (5,9): warning CS8602: Possible dereference of a null reference.
                //         A.Nested._1.Item1.ToString(); // 1
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._1.Item1").WithLocation(5, 9),
                // (7,9): warning CS8602: Possible dereference of a null reference.
                //         A.Nested._2.ToString(); // 2
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._2").WithLocation(7, 9),
                // (10,9): warning CS8602: Possible dereference of a null reference.
                //         A.Nested._4.Item1.Item1[0].ToString(); // 3
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._4.Item1.Item1[0]").WithLocation(10, 9),
                // (12,9): warning CS8602: Possible dereference of a null reference.
                //         A.Nested._4.Item2.ToString(); // 4
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._4.Item2").WithLocation(12, 9),
                // (13,9): warning CS8602: Possible dereference of a null reference.
                //         A.Long._1.ToString(); // 5
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._1").WithLocation(13, 9),
                // (15,9): warning CS8602: Possible dereference of a null reference.
                //         A.Long._3.ToString(); // 6
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._3").WithLocation(15, 9),
                // (17,9): warning CS8602: Possible dereference of a null reference.
                //         A.Long._5.ToString(); // 7
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._5").WithLocation(17, 9),
                // (19,9): warning CS8602: Possible dereference of a null reference.
                //         A.Long._7.ToString(); // 8
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._7").WithLocation(19, 9),
                // (21,9): warning CS8602: Possible dereference of a null reference.
                //         A.Long._9.ToString(); // 9
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._9").WithLocation(21, 9));

            var type = comp2.GetMember<NamedTypeSymbol>("A");
            Assert.Equal(
                "((System.Object?, System.Object) _1, System.Object? _2, System.Object _3, ((System.Object?[], System.Object), System.Object?) _4)",
                type.GetMember<FieldSymbol>("Nested").Type.ToTestDisplayString());
            Assert.Equal(
                "(System.Object? _1, System.Object _2, System.Object? _3, System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)",
                type.GetMember<FieldSymbol>("Long").Type.ToTestDisplayString());
        }

        // DynamicAttribute and NullableAttribute formats should be aligned.
        [Fact]
        public void TuplesDynamic()
        {
            var source =
@"#pragma warning disable 0067
using System;
public interface I<T> { }
public class A<T> { }
public class B<T> :
    A<(object? _1, (object _2, object? _3), object _4, object? _5, object _6, object? _7, object _8, object? _9)>,
    I<(object? _1, (object _2, object? _3), object _4, object? _5, object _6, object? _7, object _8, object? _9)>
    where T : A<(object? _1, (object _2, object? _3), object _4, object? _5, object _6, object? _7, object _8, object? _9)>
{
    public (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) Field;
    public event EventHandler<(dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9)> Event;
    public (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) Method(
        (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) arg) => arg;
    public (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) Property { get; set; }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "B`1");
                // Base type
                checkAttributesNoDynamic(typeDef.GetCustomAttributes(), addOne: true); // add one for A<T>
                // Interface implementation
                var interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                checkAttributesNoDynamic(interfaceImpl.GetCustomAttributes(), addOne: true); // add one for I<T>
                // Type parameter constraint type
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                checkAttributesNoDynamic(constraint.GetCustomAttributes(), addOne: true); // add one for A<T>
                // Field type
                var field = typeDef.GetFields().Select(f => reader.GetFieldDefinition(f)).Single(f => reader.StringComparer.Equals(f.Name, "Field"));
                checkAttributes(field.GetCustomAttributes());
                // Event type
                var @event = typeDef.GetEvents().Select(e => reader.GetEventDefinition(e)).Single(e => reader.StringComparer.Equals(e.Name, "Event"));
                checkAttributes(@event.GetCustomAttributes(), addOne: true); // add one for EventHandler<T>
                // Method return type and parameter type
                var method = typeDef.GetMethods().Select(m => reader.GetMethodDefinition(m)).Single(m => reader.StringComparer.Equals(m.Name, "Method"));
                var parameters = method.GetParameters().Select(p => reader.GetParameter(p)).ToArray();
                checkAttributes(parameters[0].GetCustomAttributes()); // return type
                checkAttributes(parameters[1].GetCustomAttributes()); // parameter
                // Property type
                var property = typeDef.GetProperties().Select(p => reader.GetPropertyDefinition(p)).Single(p => reader.StringComparer.Equals(p.Name, "Property"));
                checkAttributes(property.GetCustomAttributes());

                void checkAttributes(CustomAttributeHandleCollection customAttributes, bool addOne = false)
                {
                    AssertAttributes(reader, customAttributes,
                        "MemberReference:Void System.Runtime.CompilerServices.DynamicAttribute..ctor(Boolean[])",
                        "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                        "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
                    checkAttribute(reader.GetCustomAttribute(customAttributes.ElementAt(0)), addOne);
                    checkAttribute(reader.GetCustomAttribute(customAttributes.ElementAt(2)), addOne);
                }

                void checkAttributesNoDynamic(CustomAttributeHandleCollection customAttributes, bool addOne = false)
                {
                    AssertAttributes(reader, customAttributes,
                        "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                        "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Boolean[])");
                    checkAttribute(reader.GetCustomAttribute(customAttributes.ElementAt(1)), addOne);
                }

                void checkAttribute(CustomAttribute customAttribute, bool addOne)
                {
                    var expectedBits = ImmutableArray.Create(false, true, false, false, true, false, true, false, true, false, false, true);
                    if (addOne)
                    {
                        expectedBits = ImmutableArray.Create(false).Concat(expectedBits);
                    }
                    AssertEx.Equal(expectedBits, reader.ReadBoolArray(customAttribute.Value));
                }
            });

            var source2 =
@"class C
{
    static void F(B<A<(object?, (object, object?), object, object?, object, object?, object, object?)>> b)
    {
        b.Field._8.ToString();
        b.Field._9.ToString(); // 1
        b.Method(default)._8.ToString();
        b.Method(default)._9.ToString(); // 2
        b.Property._8.ToString();
        b.Property._9.ToString(); // 3
    }
}";
            var comp2 = CreateCompilation(source2, parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (6,9): warning CS8602: Possible dereference of a null reference.
                //         b.Field._9.ToString(); // 1
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "b.Field._9").WithLocation(6, 9),
                // (8,9): warning CS8602: Possible dereference of a null reference.
                //         b.Method(default)._9.ToString(); // 2
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "b.Method(default)._9").WithLocation(8, 9),
                // (10,9): warning CS8602: Possible dereference of a null reference.
                //         b.Property._9.ToString(); // 3
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "b.Property._9").WithLocation(10, 9));

            var type = comp2.GetMember<NamedTypeSymbol>("B");
            Assert.Equal(
                "A<(System.Object? _1, (System.Object _2, System.Object? _3), System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)>",
                type.BaseTypeNoUseSiteDiagnostics.ToTestDisplayString());
            Assert.Equal(
                "I<(System.Object? _1, (System.Object _2, System.Object? _3), System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)>",
                type.Interfaces()[0].ToTestDisplayString());
            Assert.Equal(
                "A<(System.Object? _1, (System.Object _2, System.Object? _3), System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)>",
                type.TypeParameters[0].ConstraintTypes()[0].ToTestDisplayString());
            Assert.Equal(
                "(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9)",
                type.GetMember<FieldSymbol>("Field").Type.ToTestDisplayString());
            Assert.Equal(
                "System.EventHandler<(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9)>",
                type.GetMember<EventSymbol>("Event").Type.ToTestDisplayString());
            Assert.Equal(
                "(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9) B<T>.Method((dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9) arg)",
                type.GetMember<MethodSymbol>("Method").ToTestDisplayString());
            Assert.Equal(
                "(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9) B<T>.Property { get; set; }",
                type.GetMember<PropertySymbol>("Property").ToTestDisplayString());
        }

        private static void AssertNoNullableAttribute(ImmutableArray<CSharpAttributeData> attributes)
        {
            AssertAttributes(attributes);
        }

        private static void AssertNullableAttribute(ImmutableArray<CSharpAttributeData> attributes)
        {
            AssertAttributes(attributes, "System.Runtime.CompilerServices.NullableAttribute");
        }

        private static void AssertAttributes(ImmutableArray<CSharpAttributeData> attributes, params string[] expectedNames)
        {
            var actualNames = attributes.Select(a => a.AttributeClass.ToTestDisplayString()).ToArray();
            AssertEx.SetEqual(actualNames, expectedNames);
        }

        private static void AssertAttribute(CSharpCompilation comp, string attributeName = "NullableAttribute")
        {
            var image = comp.EmitToArray();
            using (var reader = new PEReader(image))
            {
                var metadataReader = reader.GetMetadataReader();
                var attributes = metadataReader.GetCustomAttributeRows().Select(metadataReader.GetCustomAttributeName).ToArray();
                Assert.True(attributes.Contains(attributeName));
            }
        }

        private static TypeDefinition GetTypeDefinitionByName(MetadataReader reader, string name)
        {
            return reader.GetTypeDefinition(reader.TypeDefinitions.Single(h => reader.StringComparer.Equals(reader.GetTypeDefinition(h).Name, name)));
        }

        private static void AssertAttributes(MetadataReader reader, CustomAttributeHandleCollection handles, params string[] expectedNames)
        {
            var actualNames = handles.Select(h => reader.Dump(reader.GetCustomAttribute(h).Constructor)).ToArray();
            AssertEx.SetEqual(actualNames, expectedNames);
        }
    }
}