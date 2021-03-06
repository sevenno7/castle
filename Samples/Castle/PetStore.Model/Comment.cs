// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace PetStore.Model
{
	using System;

	using Castle.ActiveRecord;

	[ActiveRecord("ProductComment")]
	public class Comment : ActiveRecordBase
	{
		private int id;
		private String text;
		private DateTime postedAt;
		private Customer author;
		private Product product;

		public Comment()
		{
			postedAt = DateTime.Now;
		}

		[PrimaryKey]
		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		[Property(ColumnType="StringClob")]
		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		[BelongsTo("author_id")]
		public Customer Author
		{
			get { return author; }
			set { author = value; }
		}

		[BelongsTo("product_id")]
		public Product Product
		{
			get { return product; }
			set { product = value; }
		}

		[Property]
		public DateTime PostedAt
		{
			get { return postedAt; }
			set { postedAt = value; }
		}
	}
}
