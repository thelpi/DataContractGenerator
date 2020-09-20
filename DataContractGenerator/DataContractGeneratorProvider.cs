using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataContractGenerator
{
    /// <summary>
    /// Provider to generate data contract instances.
    /// </summary>
    public class DataContractGeneratorProvider
    {
        private static readonly IDictionary<int, Type> _tupleTypes = new Dictionary<int, Type>
        {
            { 1, typeof(Tuple<>) },
            { 2, typeof(Tuple<,>) },
            { 3, typeof(Tuple<,,>) },
            { 4, typeof(Tuple<,,,>) },
            { 5, typeof(Tuple<,,,,>) },
            { 6, typeof(Tuple<,,,,,>) },
            { 7, typeof(Tuple<,,,,,,>) },
            { 8, typeof(Tuple<,,,,,,,>) },
        };

        /// <summary>
        /// List of built-in managed types.
        /// When an interface is specified, implementations that inherit directly the interface are managed.
        /// For other types, tries the default constructor if any; otherwise a random available constructor.
        /// </summary>
        /// <remarks>
        /// Despite not being in this list, arrays are managed, as long as:
        /// <list type="bullet">
        /// <item>They're generic; ie not <see cref="Array"/>.</item>
        /// <item>They're uni-dimensional.</item>
        /// </list>
        /// </remarks>
        public static IReadOnlyCollection<Type> ManagedTypes = new List<Type>
        {
            typeof(string), typeof(decimal), typeof(sbyte), typeof(byte), typeof(ushort), typeof(short), typeof(int),
            typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(bool), typeof(double), typeof(float),
            typeof(DateTime), typeof(Enum), typeof(Nullable<>), typeof(KeyValuePair<,>), typeof(TimeSpan),
            typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>), typeof(DateTimeOffset),
            typeof(IReadOnlyDictionary<,>), typeof(IDictionary<,>), typeof(IEnumerable<>),
            typeof(IReadOnlyList<>), typeof(IList<>), typeof(ICollection<>), typeof(Guid)
        };

        private const int MAX_LIST_COUNT = 10;
        private const int MIN_LIST_COUNT = 1;
        private const int MIN_STRING_LENGTH = 3;
        private const int MAX_STRING_LENGTH = 20;
        private const string _alpha = "abcdefghijklmnopqrstuvwxyz";

        private static readonly string _alphaFull = string.Concat(_alpha, _alpha.ToUpper());
        private static readonly Random _rdm = new Random();

        private readonly Dictionary<Type, Delegate> _converters = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataContractGeneratorProvider() : this(null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="converters">
        /// List of type converters.
        /// The key is the target type.
        /// The value is the delegate to obtain the instance of the target type.
        /// </param>
        /// <remarks>
        /// Converters are applied in the order they're provided, and the converter is apply on a property that is from a child type.
        /// So make sure to provide the most specific types in first.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="converters"/> contains one or several null referenced delegates.</exception>
        public DataContractGeneratorProvider(IDictionary<Type, Delegate> converters)
        {
            if (converters != null)
            {
                if (converters.Any(c => c.Value == null))
                {
                    throw new ArgumentException("The instance contains one or several null referenced delegates.", nameof(converters));
                }
                _converters = new Dictionary<Type, Delegate>(converters);
            }
        }

        /// <summary>
        /// Generates an instance of the specified type and randomize its properties values.
        /// </summary>
        /// <typeparam name="T">Type to instanciate.</typeparam>
        /// <returns>Instance of type.</returns>
        /// <exception cref="NotSupportedException">
        /// The property type is not included in <see cref="ManagedTypes"/> or in the custom converters list, and does not have a suitable constructor.
        /// </exception>
        public T GenerateRandom<T>() where T : new()
        {
            T instance = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
            FillRandomProperties(instance);
            return instance;
        }

        /// <summary>
        /// Randomise values of each property of an instance (of any type), as long as the setter is publicly accessible.
        /// </summary>
        /// <param name="instance">The instance to fill.</param>
        /// <exception cref="NotSupportedException">
        /// The property type is not included in <see cref="ManagedTypes"/> or in the custom converters list, and does not have a suitable constructor.
        /// </exception>
        public void FillRandomProperties(object instance)
        {
            foreach (PropertyInfo property in instance.GetType().GetProperties().Where(p => p.SetMethod != null))
            {
                property.SetValue(instance, GetRandomValueForType(property.PropertyType));
            }
        }

        private object GetRandomValueForType(Type pType)
        {
            foreach (var converterType in _converters.Keys)
            {
                if (pType == converterType || pType.IsSubclassOf(converterType))
                {
                    return _converters[converterType].DynamicInvoke();
                }
            }

            if (pType == typeof(string))
            {
                return GetRandomString();
            }
            else if (pType == typeof(decimal))
            {
                return GetRandomDecimal();
            }
            else if (pType == typeof(sbyte) || pType == typeof(byte)
                 || pType == typeof(ushort) || pType == typeof(short)
                || pType == typeof(int) || pType == typeof(uint)
                || pType == typeof(long) || pType == typeof(ulong))
            {
                return GetRandomSmallInteger();
            }
            else if (pType == typeof(char))
            {
                return GetRandomChar();
            }
            else if (pType == typeof(bool))
            {
                return GetRandomBoolean();
            }
            else if (pType == typeof(double) || pType == typeof(float))
            {
                return _rdm.NextDouble();
            }
            else if (pType.IsEnum)
            {
                return GetRandomEnumValue(pType);
            }
            else if (pType == typeof(DateTime))
            {
                return GetRandomDateTime();
            }
            else if (pType == typeof(TimeSpan))
            {
                return GetRandomTimeSpan();
            }
            else if (pType == typeof(DateTimeOffset))
            {
                return GetRandomDateTimeOffset();
            }
            else if (pType == typeof(Guid))
            {
                return GetRandomGuid();
            }
            else if (Nullable.GetUnderlyingType(pType) != null)
            {
                return GetRandomNullableValue(pType);
            }
            else if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return GetRandomKeyValuePair(pType.GenericTypeArguments[0], pType.GenericTypeArguments[1]);
            }
            else if (pType.IsGenericType && (
                typeof(Tuple<>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,,,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,,,,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,,,,,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,,,,,,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(Tuple<,,,,,,,>).IsAssignableFrom(pType.GetGenericTypeDefinition())))
            {
                return GetRandomTuple(pType.GenericTypeArguments);
            }
            else if (pType.IsArray && pType.GetArrayRank() == 1)
            {
                return GetArrayOfType(pType);
            }
            else if (pType.IsGenericType && (
                typeof(IDictionary<,>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(IReadOnlyDictionary<,>).IsAssignableFrom(pType.GetGenericTypeDefinition())))
            {
                return GetGenericDictionary(pType.GenericTypeArguments[0], pType.GenericTypeArguments[1]);
            }
            else if (pType.IsGenericType && (
                typeof(IReadOnlyCollection<>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(IList<>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(ICollection<>).IsAssignableFrom(pType.GetGenericTypeDefinition())
                || typeof(IEnumerable<>).IsAssignableFrom(pType.GetGenericTypeDefinition())))
            {
                return GetGenericList(pType);
            }
            else if (pType.IsInterface)
            {
                return GetRandomConcreteInstanceFromInterface(pType);
            }
            else if (pType.GetConstructor(Type.EmptyTypes) != null)
            {
                return DynamicGenerateRandom(pType);
            }
            else if (pType.GetConstructors().Length > 0)
            {
                return DynamicComplexConstructor(pType.GetConstructors());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private object GetRandomConcreteInstanceFromInterface(Type pType)
        {
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ns => ns.GetTypes())
                .Where(t => pType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.GetConstructors().Length > 0)
                .ToList();

            if (types.Count == 0)
            {
                throw new NotSupportedException();
            }

            var selectedType = types[_rdm.Next(0, types.Count)];

            if (selectedType.GetConstructor(Type.EmptyTypes) != null)
            {
                return DynamicGenerateRandom(selectedType);
            }
            else
            {
                return DynamicComplexConstructor(selectedType.GetConstructors());
            }
        }

        private object GetRandomTuple(Type[] genericTypeArguments)
        {
            return _tupleTypes[genericTypeArguments.Length]
                .MakeGenericType(genericTypeArguments)
                .GetConstructor(genericTypeArguments)
                .Invoke(genericTypeArguments.Select(t => GetRandomValueForType(t)).ToArray());
        }

        private object GetRandomKeyValuePair(Type type1, Type type2)
        {
            return typeof(KeyValuePair<,>)
                .MakeGenericType(type1, type2)
                .GetConstructor(new Type[] { type1, type2 })
                .Invoke(new object[] { GetRandomValueForType(type1), GetRandomValueForType(type2) });
        }

        private object GetGenericDictionary(Type keyType, Type valueType)
        {
            var dict = typeof(Dictionary<,>)
                .MakeGenericType(keyType, valueType)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);

            var emptyListAdd = dict.GetType().GetMethod(nameof(System.Collections.IDictionary.Add));
            int maxKeysCount = _rdm.Next(1, MAX_LIST_COUNT + 1);
            var keys = new List<object>();
            do
            {
                var keyValue = GetRandomValueForType(keyType);
                if (keys.Contains(keyValue))
                {
                    break;
                }
                keys.Add(keyValue);
                emptyListAdd.Invoke(dict, new object[] { keyValue, GetRandomValueForType(valueType) });
            }
            while (keys.Count < maxKeysCount);

            return dict;
        }

        private object GetArrayOfType(Type type)
        {
            int size = _rdm.Next(MIN_LIST_COUNT, MAX_LIST_COUNT + 1);
            var actualValues = Array.CreateInstance(type.GetElementType(), size);
            for (int i = 0; i < size; i++)
            {
                actualValues.SetValue(GetRandomValueForType(type.GetElementType()), i);
            }
            return actualValues;
        }

        private object GetRandomNullableValue(Type pType)
        {
            return GetRandomBoolean() ? null :
                Convert.ChangeType(GetRandomValueForType(Nullable.GetUnderlyingType(pType)),
                    Nullable.GetUnderlyingType(pType));
        }

        private object GetGenericList(Type pType)
        {
            var underlyingType = pType.GenericTypeArguments.First();

            var listOfPropType = typeof(List<>)
                .MakeGenericType(underlyingType)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);

            var addMethod = listOfPropType.GetType().GetMethod(nameof(System.Collections.IList.Add));

            for (int i = 0; i < _rdm.Next(MIN_LIST_COUNT, MAX_LIST_COUNT + 1); i++)
            {
                object elementInstance = DynamicGenerateRandom(underlyingType);

                addMethod.Invoke(listOfPropType, new object[] { elementInstance });
            }

            return listOfPropType;
        }

        private object DynamicComplexConstructor(ConstructorInfo[] constructors)
        {
            ConstructorInfo ctor = constructors[_rdm.Next(0, constructors.Length)];
            var ctorParameters = new List<object>();
            foreach (ParameterInfo pInfo in ctor.GetParameters())
            {
                ctorParameters.Add(GetRandomValueForType(pInfo.ParameterType));
            }
            object instance = ctor.Invoke(ctorParameters.ToArray());
            FillRandomProperties(instance);
            return instance;
        }

        private object DynamicGenerateRandom(Type type)
        {
            return GetType()
                .GetMethod(nameof(GenerateRandom))
                .MakeGenericMethod(type)
                .Invoke(this, null);
        }

        #region Struct and primitive types instanciation

        private static Guid GetRandomGuid()
        {
            return Guid.NewGuid();
        }

        private static DateTimeOffset GetRandomDateTimeOffset()
        {
            return new DateTimeOffset(GetRandomDateTime(), GetRandomTimeSpan());
        }

        private static TimeSpan GetRandomTimeSpan()
        {
            return new TimeSpan(_rdm.Next(0, int.MaxValue), _rdm.Next(0, 24), _rdm.Next(0, 60), _rdm.Next(0, 60), _rdm.Next(0, 1000));
        }

        private static DateTime GetRandomDateTime()
        {
            List<DateTimeKind> dtk = Enum.GetValues(typeof(DateTimeKind)).Cast<DateTimeKind>().ToList();
            return new DateTime(_rdm.Next(0, 3000),
                _rdm.Next(1, 13),
                _rdm.Next(1, 29),
                _rdm.Next(0, 24),
                _rdm.Next(0, 60),
                _rdm.Next(0, 60),
                _rdm.Next(0, 1000),
                dtk[_rdm.Next(0, dtk.Count)]);
        }

        private static object GetRandomEnumValue(Type pType)
        {
            // Dirty, but values from non-generic Array can't be accessed by [] or Linq
            Array values = Enum.GetValues(pType);
            int stopTo = _rdm.Next(0, values.Length);
            int i = 0;
            foreach (object value in values)
            {
                if (i == stopTo)
                {
                    return value;
                }
                i++;
            }
            return null;
        }

        private static decimal GetRandomDecimal()
        {
            return GetRandomSmallInteger() + (GetRandomSmallInteger() / (decimal)(sbyte.MaxValue + 1));
        }

        private static sbyte GetRandomSmallInteger()
        {
            return (sbyte)_rdm.Next(0, sbyte.MaxValue + 1);
        }

        private static bool GetRandomBoolean()
        {
            return _rdm.Next(0, 2) == 1;
        }

        private static string GetRandomString()
        {
            return string.Concat(
                Enumerable
                    .Range(0, _rdm.Next(MIN_STRING_LENGTH, MAX_STRING_LENGTH + 1))
                    .Select(i => GetRandomChar())
            );
        }

        private static char GetRandomChar()
        {
            return _alphaFull[_rdm.Next(0, _alphaFull.Length)];
        }

        #endregion
    }
}
