using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Utilities.Helpers
{
    //Some helper methods to easily find entity configurations
    public static class EntityTypeConfigurationExtensions
    {
        private static readonly Dictionary<Assembly, IEnumerable<Type>> typesPerAssembly = new();
        public static ModelBuilder UseEntityTypeConfiguration(this ModelBuilder modelBuilder)
        {
            var asm = Assembly.GetCallingAssembly();

            if (!typesPerAssembly.TryGetValue(asm, out var configurationTypes))
            {
                typesPerAssembly[asm] = configurationTypes = asm
                    .GetTypes()
                    .Where(x => x.GetTypeInfo().IsClass
                                && !x.GetTypeInfo().IsAbstract
                                && x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType
                                                              && y.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));
            }

            var configurations = configurationTypes.Select(Activator.CreateInstance);

            foreach (dynamic? configuration in configurations)
            {
                ApplyConfiguration(modelBuilder, configuration);
            }

            return modelBuilder;
        }

        private static void ApplyConfiguration<T>(this ModelBuilder modelBuilder, IEntityTypeConfiguration<T> configuration) where T : class
        {
            var entityType = FindEntityType(configuration.GetType());

            dynamic entityTypeBuilder = entityMethod
                .MakeGenericMethod(entityType)
                .Invoke(modelBuilder, Array.Empty<object>())!;

            configuration.Configure(entityTypeBuilder);
        }

        private static readonly MethodInfo entityMethod = typeof(ModelBuilder)
            .GetTypeInfo()
            .GetMethods()
            .Single(x => x.Name == "Entity" && x.IsGenericMethod && x.GetParameters().Length == 0);

        private static Type FindEntityType(Type type)
        {
            var interfaceType = type.GetInterfaces()
                .First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
            return interfaceType.GetGenericArguments().First();
        }
    }
}
