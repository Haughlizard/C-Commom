using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithms
{
    /// <summary>
    /// 链表实现的下压栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Stack<T>
    {
        private class Node 
        {
            public T t;
            public Node next;
        }

        private Node first;
        private int N = 0;

        public bool isEmpty() { return first == null; }
        public int size() { return N; }

        public void push(T item)
        {
            if (first == null)
            {
                first = new Node();
                first.t = item;
            }
            else
            {
                Node oldfirst = first;
                first = new Node();
                first.t = item;
                first.next = oldfirst;
            }
            N++;
        }

        public T pop()
        {
            if(first == null) throw new NullReferenceException();
            T item = first.t;
            first = first.next;
            N--;
            return item;
        }
    }
}
