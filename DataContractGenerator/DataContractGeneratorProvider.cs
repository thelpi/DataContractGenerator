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
        // Can't make this dynamic: number of arguments expected for each kind of tuple
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
        /// </summary>
        /// <remarks>
        /// Despite not being in this list:
        /// Every generic types that inherit from <see cref="System.Collections.IEnumerable"/>
        /// will be managed as <see cref="List{T}"/> or <see cref="Dictionary{TKey, TValue}"/>,
        /// depending on the number of underlying generic types.
        /// This implementation might fail for specific type such as <see cref="HashSet{T}"/> or <see cref="SortedList{TKey, TValue}"/>.
        /// Arrays are managed, as long as they're generic.
        /// </remarks>
        public static IReadOnlyCollection<Type> ManagedTypes = new List<Type>
        {
            typeof(string), typeof(decimal), typeof(sbyte), typeof(byte), typeof(ushort), typeof(short), typeof(int),
            typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(bool), typeof(double), typeof(float),
            typeof(DateTime), typeof(Enum), typeof(Nullable<>), typeof(KeyValuePair<,>), typeof(TimeSpan),
            typeof(Tuple<>), typeof(Tuple<,,,,,,,>), typeof(DateTimeOffset), typeof(Guid)
        };

        private const string _alpha = "abcdefghijklmnopqrstuvwxyz";

        private static readonly string _alphaFull = string.Concat(_alpha, _alpha.ToUpper());
        private static readonly Random _rdm = new Random();

        private readonly GenerationOptions _options;
        private readonly ILogger _logger;
        private readonly Dictionary<Type, Delegate> _converters = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataContractGeneratorProvider() : this(null, null, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="converters">
        /// List of type converters.
        /// The key is the target type.
        /// The value is the delegate to obtain the instance of the target type.
        /// </param>
        /// <param name="logger">Logger.</param>
        /// <param name="options">Options.</param>
        /// <remarks>
        /// Converters are applied in the order they're provided, and the converter is apply on a property that is from a child type.
        /// So make sure to provide the most specific types in first.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="converters"/> contains one or several null referenced delegates.</exception>
        public DataContractGeneratorProvider(ILogger logger, IDictionary<Type, Delegate> converters, GenerationOptions options)
        {
            _logger = logger ?? new DefaultLogger();
            _options = options ?? new GenerationOptions();
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
        public T GenerateRandom<T>() where T : new()
        {
            return GenerateRandomInternal<T>(0);
        }

        private T GenerateRandomInternal<T>(int depth) where T : new()
        {
            T instance = new T();
            FillRandomProperties(instance, depth);
            return instance;
        }

        private void FillRandomProperties(object instance, int depth)
        {
            if (depth < _options.MaximalRecursionDepth)
            {
                foreach (PropertyInfo property in instance.GetType().GetProperties().Where(p => p.SetMethod != null))
                {
                    try
                    {
                        object value = GetRandomValueForType(property.PropertyType, depth);
                        property.SetValue(instance, value);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ex);
                    }
                }
            }
        }

        private object GetRandomValueForType(Type pType, int depth)
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
            else if (pType == typeof(sbyte) || pType == typeof(short)
                || pType == typeof(int) || pType == typeof(long))
            {
                return GetRandomInteger();
            }
            else if (pType == typeof(byte) || pType == typeof(ushort)
                || pType == typeof(uint) || pType == typeof(ulong))
            {
                return GetRandomUnsignedInteger();
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
                return GetRandomFloat();
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
                return GetRandomNullableValue(pType, depth);
            }
            else if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return GetRandomKeyValuePair(pType.GenericTypeArguments[0], pType.GenericTypeArguments[1], depth);
            }
            else if (pType.IsGenericType && typeof(System.Runtime.CompilerServices.ITuple).IsAssignableFrom(pType.GetGenericTypeDefinition()))
            {
                return GetRandomTuple(pType.GenericTypeArguments, depth);
            }
            else if (pType.IsArray)
            {
                return GetArrayOfType(pType, depth);
            }
            else if (pType.IsGenericType
                && pType.GenericTypeArguments.Length == 2
                && typeof(System.Collections.IEnumerable).IsAssignableFrom(pType))
            {
                return GetGenericDictionary(pType.GenericTypeArguments[0], pType.GenericTypeArguments[1], depth);
            }
            else if (pType.IsGenericType
                && pType.GenericTypeArguments.Length == 1
                && typeof(System.Collections.IEnumerable).IsAssignableFrom(pType))
            {
                return GetGenericList(pType, depth);
            }
            else if (pType.GetConstructor(Type.EmptyTypes) != null)
            {
                return DynamicGenerateRandom(pType, depth);
            }
            else if (pType.GetConstructors().Length > 0)
            {
                return DynamicComplexConstructor(pType.GetConstructors(), depth);
            }
            else
            {
                // Assumes at this point the property type is:
                // - an interface
                // - an abstract class
                // - a concrete class without public constructor
                // in any case, tries to find a inherited concrete type
                // gets null if everything fails
                return GetRandomConcreteInstanceFromAbstraction(pType, depth);
            }
        }

        private object GetRandomConcreteInstanceFromAbstraction(Type pType, int depth)
        {
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ns => ns.GetTypes())
                .Where(t => pType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.GetConstructors().Length > 0)
                .ToList();

            if (types.Count > 0)
            {
                return GetRandomValueForType(types[_rdm.Next(0, types.Count)], depth);
            }

            return null;
        }

        private object GetRandomTuple(Type[] genericTypeArguments, int depth)
        {
            return _tupleTypes[genericTypeArguments.Length]
                .MakeGenericType(genericTypeArguments)
                .GetConstructor(genericTypeArguments)
                .Invoke(genericTypeArguments.Select(t => GetRandomValueForType(t, depth)).ToArray());
        }

        private object GetRandomKeyValuePair(Type type1, Type type2, int depth)
        {
            return typeof(KeyValuePair<,>)
                .MakeGenericType(type1, type2)
                .GetConstructor(new Type[] { type1, type2 })
                .Invoke(new object[] { GetRandomValueForType(type1, depth), GetRandomValueForType(type2, depth) });
        }

        private object GetGenericDictionary(Type keyType, Type valueType, int depth)
        {
            var dict = typeof(Dictionary<,>)
                .MakeGenericType(keyType, valueType)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);

            var emptyListAdd = dict.GetType().GetMethod(nameof(System.Collections.IDictionary.Add));
            int maxKeysCount = _rdm.Next(1, _options.MaxListCount + 1);
            var keys = new List<object>();
            do
            {
                var keyValue = GetRandomValueForType(keyType, depth);
                if (keys.Contains(keyValue))
                {
                    break;
                }
                keys.Add(keyValue);
                emptyListAdd.Invoke(dict, new object[] { keyValue, GetRandomValueForType(valueType, depth) });
            }
            while (keys.Count < maxKeysCount);

            return dict;
        }

        private object GetArrayOfType(Type type, int depth)
        {
            int dims = type.GetArrayRank();

            int size = _rdm.Next(_options.MinListCount, _options.MaxListCount + 1);

            // Note: every dimensions have the same size
            int[] sizeForEachDim = Enumerable.Range(0, dims).Select(i => size).ToArray();

            var array = Array.CreateInstance(type.GetElementType(), sizeForEachDim);
            
            RecursiveArrayFill(type, depth, size, array, new int[dims], 0, dims);

            return array;
        }

        private void ArrayFillStep(Type arrayInnerType, int instanciationDepth, int countElementsByDimension,
            Array array, int[] currentArrayPosition, int currentDimension)
        {
            for (int i = 0; i < countElementsByDimension; i++)
            {
                currentArrayPosition[currentDimension] = i;
                array.SetValue(GetRandomValueForType(arrayInnerType.GetElementType(), instanciationDepth), currentArrayPosition);
            }
        }

        private void RecursiveArrayFill(Type arrayInnerType, int instanciationDepth, int countElementsByDimension,
            Array array, int[] currentArrayPosition, int currentDimension, int countDimensions)
        {
            if (currentDimension == countDimensions - 1)
            {
                ArrayFillStep(arrayInnerType, instanciationDepth, countElementsByDimension, array, currentArrayPosition, currentDimension);
            }
            else
            {
                for (int dim = 0; dim < countElementsByDimension; dim++)
                {
                    currentArrayPosition[currentDimension] = dim;
                    RecursiveArrayFill(arrayInnerType, instanciationDepth, countElementsByDimension, array, currentArrayPosition, currentDimension + 1, countDimensions);
                }
            }
        }

        private object GetRandomNullableValue(Type type, int depth)
        {
            return Convert.ChangeType(GetRandomValueForType(Nullable.GetUnderlyingType(type), depth), Nullable.GetUnderlyingType(type));
        }

        private object GetGenericList(Type type, int depth)
        {
            object listOfPropType = typeof(List<>)
                .MakeGenericType(type.GenericTypeArguments[0])
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);

            MethodInfo addMethod = listOfPropType.GetType().GetMethod(nameof(System.Collections.IList.Add));

            for (int i = 0; i < _rdm.Next(_options.MinListCount, _options.MaxListCount + 1); i++)
            {
                addMethod.Invoke(listOfPropType, new object[] { GetRandomValueForType(type.GenericTypeArguments[0], depth) });
            }

            return listOfPropType;
        }

        private object DynamicComplexConstructor(ConstructorInfo[] constructors, int depth)
        {
            ConstructorInfo ctor = constructors[_rdm.Next(0, constructors.Length)];
            var ctorParameters = new List<object>();
            foreach (ParameterInfo pInfo in ctor.GetParameters())
            {
                ctorParameters.Add(GetRandomValueForType(pInfo.ParameterType, depth));
            }
            object instance = ctor.Invoke(ctorParameters.ToArray());
            FillRandomProperties(instance, depth + 1);
            return instance;
        }

        private object DynamicGenerateRandom(Type type, int depth)
        {
            return GetType()
                .GetMethod(nameof(GenerateRandomInternal), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { depth + 1 });
        }

        #region System types instanciation

        private float GetRandomFloat()
        {
            double mantissa = (_rdm.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, _rdm.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

        private Guid GetRandomGuid()
        {
            return Guid.NewGuid();
        }

        private DateTimeOffset GetRandomDateTimeOffset()
        {
            return new DateTimeOffset(GetRandomDateTime(DateTimeKind.Unspecified), new TimeSpan(0, _rdm.Next(0, 60), 0));
        }

        private TimeSpan GetRandomTimeSpan()
        {
            return new TimeSpan(_rdm.Next(0, TimeSpan.MaxValue.Days), _rdm.Next(0, 24), _rdm.Next(0, 60), _rdm.Next(0, 60), _rdm.Next(0, 1000));
        }

        private DateTime GetRandomDateTime(DateTimeKind? dtk = null)
        {
            List<DateTimeKind> dtks = Enum.GetValues(typeof(DateTimeKind)).Cast<DateTimeKind>().ToList();
            return new DateTime(_rdm.Next(1, 3000),
                _rdm.Next(1, 13),
                _rdm.Next(1, 29),
                _rdm.Next(0, 24),
                _rdm.Next(0, 60),
                _rdm.Next(0, 60),
                _rdm.Next(0, 1000),
                dtk ?? dtks[_rdm.Next(0, dtks.Count)]);
        }

        private object GetRandomEnumValue(Type type)
        {
            // Dirty, but values from non-generic Array can't be accessed by [] or Linq
            Array values = Enum.GetValues(type);
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

        private decimal GetRandomDecimal()
        {
            return GetRandomInteger() + (GetRandomUnsignedInteger() / (decimal)(byte.MaxValue + 1));
        }

        private byte GetRandomUnsignedInteger()
        {
            return (byte)_rdm.Next(byte.MinValue, byte.MaxValue + 1);
        }

        private sbyte GetRandomInteger()
        {
            return (sbyte)_rdm.Next(sbyte.MinValue, sbyte.MaxValue + 1);
        }

        private bool GetRandomBoolean()
        {
            return _rdm.Next(0, 2) == 1;
        }

        private string GetRandomString()
        {
            return string.Concat(
                Enumerable
                    .Range(0, _rdm.Next(_options.MinStringLength, _options.MaxStringLength + 1))
                    .Select(i => GetRandomChar())
            );
        }

        private char GetRandomChar()
        {
            return _alphaFull[_rdm.Next(0, _alphaFull.Length)];
        }

        #endregion
    }
}
