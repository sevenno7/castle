using System.Reflection.Emit;
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

namespace Castle.DynamicProxy.Builder.CodeBuilder.SimpleAST
{
	using System;

	/// <summary>
	/// Summary description for ReferencesToObjectArrayExpression.
	/// </summary>
	public class ReferencesToObjectArrayExpression : Expression
	{
		private TypeReference[] m_args;

		public ReferencesToObjectArrayExpression( params TypeReference[] args )
		{
			m_args = args;
		}

		public override void Emit(IEasyMember member, ILGenerator gen)
		{
			LocalBuilder local = gen.DeclareLocal( typeof(object[]) );
			gen.Emit(OpCodes.Ldc_I4, m_args.Length);
			gen.Emit(OpCodes.Newarr, typeof(object));
			gen.Emit(OpCodes.Stloc, local);
			
			for(int i=0; i < m_args.Length; i++)
			{
				gen.Emit(OpCodes.Ldloc, local);
				gen.Emit(OpCodes.Ldc_I4, i);

				TypeReference reference = m_args[i];

				m_args[i].LoadReference( gen );

				if (reference.Type.IsValueType)
				{
					gen.Emit(OpCodes.Box, reference.Type.UnderlyingSystemType);
				}
				
				gen.Emit(OpCodes.Stelem_Ref);
			}

			gen.Emit(OpCodes.Ldloc, local);
		}
	}
}
