using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlayLogic.PointLogic
{
    public class Point : MonoBehaviour
    {
        public int PointID
        {
            get;
            set;
        }

        public Color PointColor
        {
            get
            {
                if (Checkers.Count == 0)
                    return Color.None;

                return Checkers.Peek().CheckerColor;
            }
        }

        public int CountOfCheckers
        {
            get
            {
                return Checkers.Count;
            }
        }

        public Checker SelectChecker
        {
            get
            {
                return Checkers.Peek();
            }
        }

        private Stack<Checker> Checkers = new Stack<Checker>();


        public void AddChecker(Checker Checker)
        {
            Checkers.Push(Checker);
        }

        public void RemoveChecker(Checker Checker)
        {
            if (Checkers.Count == 0 || !Checkers.Contains(Checker))
                return;

            Stack<Checker> tempStack = new Stack<Checker>();
            while (Checkers.Count != 0)
            {
                if (Checkers.Peek() == Checker)
                {
                    Checkers.Pop();
                    break;
      
                }
                else
                    tempStack.Push(Checkers.Pop());

            }

            while(tempStack.Count!=0)
                Checkers.Push(tempStack.Pop());
            
        }

        public Checker RemoveChecker()
        {
            if (Checkers.Count == 0)
                return null;

            return Checkers.Pop();
        }

    }
}