## Copyright 2004-2006 Castle Project - http:##www.castleproject.org/
## 
## Licensed under the Apache License, Version 2.0 (the "License");
## you may not use this file except in compliance with the License.
## You may obtain a copy of the License at
## 
##     http:##www.apache.org/licenses/LICENSE-2.0
## 
## Unless required by applicable law or agreed to in writing, software
## distributed under the License is distributed on an "AS IS" BASIS,
## WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
## See the License for the specific language governing permissions and
## limitations under the License.

require '../common'

class ProductLicense

	class << self

		include Test::Unit::Assertions

		def create(ie, created, expires)

			ie.goto('http://localhost:88/ProductLicense/new.castle')

			ie.text_field(:id, "pl_created").set(created)
			ie.text_field(:id, "pl_Expires").set(expires)
			
			ie.button(:id, 'insertbutton').click

			# Check for errors
			
			fail('There were databind errors') if ie.contains_text('binding error')
			fail('Unexpected exception') if ie.contains_text('Exception')
			
			assert_equal(created, ie.span(:id, 'created').text, 'Insert: created was not set')
			assert_equal(expires, ie.span(:id, 'expires').text, 'Insert: expires was not set')

			# returns new id
			
			ie.span(:id, 'newid').text
			
		end
		
		def edit(ie, id, created, expires)

			ie.goto("http://localhost:88/ProductLicense/edit.castle?id=#{id}")

			ie.text_field(:id, "pl_created").set(created)
			ie.text_field(:id, "pl_Expires").set(expires)
			
			ie.button(:id, 'updatebutton').click

			# Check for errors
			
			fail('There were databind errors') if ie.contains_text('binding error')
			fail('Unexpected exception') if ie.contains_text('Exception')
			
			assert_equal(created, ie.span(:id, 'created').text, 'Insert: created was not set')
			assert_equal(expires, ie.span(:id, 'expires').text, 'Insert: expires was not set')
			
		end
		
		def delete(ie, id)

			ie.goto("http://localhost:88/ProductLicense/removeconfirm.castle?id=#{id}")

			fail('Looks like removeConfirm.castle didnt load instance') unless ie.contains_text("Confirm removal of #{id}?")
			
			ie.button(:id, 'button').click

			fail('Error removing instance') unless ie.contains_text('Removed')
			fail('Unexpected exception') if ie.contains_text('Exception')
			
		end

	end

end