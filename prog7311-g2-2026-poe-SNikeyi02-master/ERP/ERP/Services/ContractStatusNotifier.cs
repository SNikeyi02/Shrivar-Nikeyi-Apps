using ERP.Interface;
using ERP.Models;
using System.Collections.Generic;

//Code attribution
//Observer design pattern
//Available at: https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern
//[Accessed: 1 June 2026]

//Code attribution
//Dependency injection guidelines
//Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/guidelines
//[Accessed: 1 June 2026]

namespace ERP.Services
{
    public class ContractStatusNotifier
    {
        private readonly List<IContractObserver> _observers = new List<IContractObserver>();

        public void Attach(IContractObserver observer) => _observers.Add(observer);

        // This method is called whenever a contract's status changes, and it notifies all attached observers of the change.
        public void Notify(Contract contract)
        {
            foreach (var observer in _observers)
            {
                observer.Update(contract);
            }
        }
    }

    // This class simulates an observer that would log the status change or "send an email" when notified of a contract status change.
    public class AdminNotificationObserver : IContractObserver
    {
        public void Update(Contract contract)
        {
            System.Diagnostics.Debug.WriteLine($"ALERT: Contract {contract.ContractId} status changed to {contract.Status}");
        }
    }
}