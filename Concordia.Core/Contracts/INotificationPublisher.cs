namespace Concordia.Contracts;

/// <summary>
/// Definisce la strategia per la pubblicazione delle notifiche a più handler.
/// </summary>
public interface INotificationPublisher
{   
    /// <summary>
    /// Pubblica una notifica a una collezione di handler.
    /// </summary>
    /// <param name="handlerCalls">Una collezione di funzioni che, se invocate, chiameranno il metodo Handle di un handler di notifica.</param>
    /// <param name="notification">La notifica da pubblicare.</param>
    /// <param name="cancellationToken">Un token per annullare l'operazione.</param>
    /// <returns>Una task che rappresenta l'operazione asincrona di pubblicazione.</returns>
    Task Publish(IEnumerable<Func<INotification, CancellationToken, Task>> handlerCalls, INotification notification, CancellationToken cancellationToken);
}
