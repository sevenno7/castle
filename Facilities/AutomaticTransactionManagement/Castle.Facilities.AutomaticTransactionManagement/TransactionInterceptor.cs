// Copyright 2004 DigitalCraftsmen - http://www.digitalcraftsmen.com.br/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilties.AutomaticTransactionManagement
{
	using System;

	using Castle.Model.Interceptor;

	using Castle.Services.Transaction;

	/// <summary>
	/// Summary description for TransactionInterceptor.
	/// </summary>
	public class TransactionInterceptor : IMethodInterceptor
	{
		private ITransactionManager _manager;

		public TransactionInterceptor(ITransactionManager manager)
		{
            _manager = manager;
		}

		public object Intercept(IMethodInvocation invocation, params object[] args)
		{
			if (!invocation.Method.IsDefined( typeof(TransactionAttribute), true ))
			{
				return invocation.Proceed(args);
			}
			else
			{
				object[] attrs = invocation.Method.GetCustomAttributes( 
					typeof(TransactionAttribute), true );

				TransactionAttribute transactionAtt = (TransactionAttribute) attrs[0];

				ITransaction transaction = 
					_manager.CreateTransaction( 
						transactionAtt.TransactionMode, transactionAtt.IsolationMode );

				object value = null;

				transaction.Begin();

				try
				{
					value = invocation.Proceed(args);

					transaction.Commit();
				}
				catch(Exception ex)
				{
					transaction.Rollback();

					throw ex;
				}
				finally
				{
					_manager.Dispose(transaction); 
				}

				return value;
			}
		}
	}
}
