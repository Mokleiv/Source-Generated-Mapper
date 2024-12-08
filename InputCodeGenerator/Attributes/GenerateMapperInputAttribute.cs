namespace InputCodeGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GenerateMapperInputAttribute<T> : Attribute;