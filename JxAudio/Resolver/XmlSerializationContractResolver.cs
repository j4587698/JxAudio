using System.Reflection;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JxAudio.Resolver;

public class XmlSerializationContractResolver: DefaultContractResolver
{
    public static readonly XmlSerializationContractResolver Instance = new XmlSerializationContractResolver();

        private XmlSerializationContractResolver()
        {
            IgnoreIsSpecifiedMembers = true;
            IgnoreSerializableAttribute = true;
            IgnoreSerializableInterface = true;
            IgnoreShouldSerializeMembers = true;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (memberSerialization != MemberSerialization.OptOut)
                throw new ArgumentException("Only opt-out is supported.", nameof(memberSerialization));

            if (type.GetCustomAttribute<XmlTypeAttribute>() == null)
                throw new InvalidOperationException();

            var jsonProperties = new List<JsonProperty>();

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                    continue;

                var choiceAttribute = field.GetCustomAttribute<XmlChoiceIdentifierAttribute>();
                var attributeAttributes = field.GetCustomAttributes<XmlAttributeAttribute>().ToArray();
                var elementAttributes = field.GetCustomAttributes<XmlElementAttribute>().ToArray();
                var textAttribute = field.GetCustomAttribute<XmlTextAttribute>();

                FieldInfo? choiceField = null;

                if (choiceAttribute == null)
                {
                    if (attributeAttributes.Length + elementAttributes.Length > 1)
                        throw new InvalidOperationException();
                }
                else
                {
                    choiceField = type.GetField(choiceAttribute.MemberName, BindingFlags.Public | BindingFlags.Instance);
                    if (choiceField == null)
                        throw new InvalidOperationException();
                }

                var names = new HashSet<string>();

                foreach (var attributeAttribute in attributeAttributes)
                {
                    var jsonProperty = CreateXmlProperty(field, memberSerialization, attributeAttribute.AttributeName, attributeAttribute.Type, choiceField);

                    if (jsonProperty.PropertyName == null || !names.Add(jsonProperty.PropertyName))
                        throw new InvalidOperationException();

                    jsonProperties.Add(jsonProperty);
                }

                foreach (var elementAttribute in elementAttributes)
                {
                    var jsonProperty = CreateXmlProperty(field, memberSerialization, elementAttribute.ElementName, elementAttribute.Type, choiceField);

                    if (jsonProperty.PropertyName == null || !names.Add(jsonProperty.PropertyName))
                        throw new InvalidOperationException();

                    jsonProperties.Add(jsonProperty);
                }

                if (textAttribute != null)
                {
                    var jsonProperty = CreateXmlTextProperty(field, memberSerialization);

                    jsonProperties.Add(jsonProperty);
                }
                else if (attributeAttributes.Length == 0 && elementAttributes.Length == 0)
                {
                    var jsonProperty = CreateXmlProperty(field, memberSerialization);

                    jsonProperties.Add(jsonProperty);
                }
            }

            return jsonProperties;
        }

        private JsonProperty CreateXmlProperty(FieldInfo field, MemberSerialization memberSerialization, string? name = null, Type? type = null, FieldInfo? choiceField = null)
        {
            var jsonProperty = CreateProperty(field, memberSerialization);

            if (!string.IsNullOrEmpty(name))
                jsonProperty.PropertyName = name;

            if (type != null)
                jsonProperty.PropertyType = type;

            if (choiceField != null)
            {
                if (!choiceField.FieldType.IsEnum)
                    throw new InvalidOperationException();

                jsonProperty.ShouldSerialize =
                    instance =>
                    {
                        if (field.GetValue(instance) == null)
                            return false;

                        var choiceValue = choiceField.GetValue(instance);
                        var choiceName = Enum.GetName(choiceField.FieldType, choiceValue ?? "");
                        return choiceName == jsonProperty.PropertyName;
                    };
            }
            else
            {
                jsonProperty.ShouldSerialize =
                    instance => field.GetValue(instance) != null;
            }

            var specifiedField = field.DeclaringType?.GetField(jsonProperty.PropertyName + "Specified", BindingFlags.Public | BindingFlags.Instance);
            if (specifiedField != null)
            {
                if (specifiedField.FieldType != typeof(bool))
                    throw new InvalidOperationException();

                var specifiedFieldIgnoreAttribute = specifiedField.GetCustomAttribute<XmlIgnoreAttribute>();
                if (specifiedFieldIgnoreAttribute != null)
                {
                    jsonProperty.GetIsSpecified =
                        instance => (bool)(specifiedField.GetValue(instance) ?? false);
                }
            }

            return jsonProperty;
        }

        private JsonProperty CreateXmlTextProperty(FieldInfo field, MemberSerialization memberSerialization)
        {
            if (field.FieldType != typeof(string[]))
                throw new InvalidOperationException();

            var jsonProperty = CreateProperty(field, memberSerialization);

            jsonProperty.PropertyName = "value";
            jsonProperty.PropertyType = typeof(string);
            jsonProperty.ValueProvider = new XmlTextPropertyValueProvider(field);

            return jsonProperty;
        }

        private class XmlTextPropertyValueProvider : IValueProvider
        {
            private readonly Func<object, object?> _getter;

            public XmlTextPropertyValueProvider(FieldInfo field)
            {
                _getter = field.GetValue;
            }

            public object? GetValue(object target)
            {
                object? value = _getter(target);
                if (value == null)
                    return null;

                var strings = (string[])value;
                return string.Join(string.Empty, strings);
            }

            public void SetValue(object target, object? value)
            {
                throw new NotImplementedException();
            }
        }
}