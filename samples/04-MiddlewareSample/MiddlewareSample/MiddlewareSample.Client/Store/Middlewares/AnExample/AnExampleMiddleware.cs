using System;
using System.Threading.Tasks;
using Blazor.Fluxor;

namespace MiddlewareSample.Client.Store.Middlewares.AnExample
{
	public class AnExampleMiddleware: Middleware
	{
		public override string GetClientScripts()
		{
			return "alert('AnExampleMiddleware script inserted successfully');";
		}

		public override Task InitializeAsync(IStore store) => base.InitializeAsync(store);

		public override void AfterInitializeAllMiddlewares()
		{
			base.AfterInitializeAllMiddlewares();
		}

		public override bool MayDispatchAction(object action)
		{
			Console.WriteLine($"Action {action.GetType().Name} has been allowed to execute");
			return true;
		}

		public override void BeforeDispatch(object action)
		{
			Console.WriteLine($"Action {action.GetType().Name} is about to be dispatched to all features");
		}

		public override void AfterDispatch(object action)
		{
			Console.WriteLine($"Action {action.GetType().Name} has just been dispatched to all features");
		}
	}
}
