namespace Nexit.Tests.Functional;

/// <summary>
/// Todas las pruebas funcionales comparten UN solo contenedor de Postgres (más rápido que uno por
/// clase) y corren SIN paralelismo entre sí -- varias clases escribiendo/leyendo en la misma base de
/// datos real al mismo tiempo darían resultados no deterministas. Las pruebas unitarias y de
/// integración (colecciones separadas) no se ven afectadas por esto y siguen corriendo en paralelo
/// como antes.
/// </summary>
[CollectionDefinition("Funcional", DisableParallelization = true)]
public class FunctionalTestCollection : ICollectionFixture<NexitFunctionalApiFactory>;
