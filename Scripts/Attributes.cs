/*
The MIT License (MIT)

Copyright (c) 2015 Sergey Sychov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Reflection;

namespace BehaviourInject
{
    public class InjectAttribute : Attribute
    { }

	/// <summary>
	/// This attribute allows to create dependency on place instead of injecting it from context.
	/// Type of created object might be not registered, but all of it's dependencies has to be registered.
	/// Each member with this attribute will obtain it's own unique newely created object
	/// </summary>
    public class CreateAttribute : Attribute
    { }

	public class InjectEventAttribute : Attribute
	{
		//Defines if event ancestors is valid event receivers.
		public bool Inherit { get; set; }
	}


	internal static class AttributeUtils
	{
		public static bool IsMarked<T>(ICustomAttributeProvider member)
		{
			return member.IsDefined(typeof(T), false);
		}

		public static bool TryGetAttribute<T>(ICustomAttributeProvider member, out T t)
		{
			object[] attributes = member.GetCustomAttributes(typeof(T), false);
			bool hasAttributes = attributes.Length > 0;

			if (hasAttributes)
				t = (T)attributes[0];
			else
				t = default(T);

			return hasAttributes;
		}
	}
}
