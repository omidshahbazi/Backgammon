using Simulation.Data.Mutation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DetermiisticSoftLayer
{
    public delegate void BarToBoardMoveMutationHandler(BarToBoardMoveMutation MutationData);
    public delegate void BoardToBarMoveMutationHandler(BoardToBarMoveMutation MutationData);
    public delegate void BoardToBoardMoveMutationHandler(BoardToBoardMoveMutation MutationData);
    public delegate void DiceChangedMutationHandler(DiceChangedMutation MutationData);

    public static partial class DelegateEvents
    {
        public static event BarToBoardMoveMutationHandler OnBarToBoardMove;
        public static event BoardToBarMoveMutationHandler OnBoardToBarMove;
        public static event BoardToBoardMoveMutationHandler OnBoardToBoardMove;
        public static event DiceChangedMutationHandler OnDiceChanged;


        public static void OnBarToBoardCall(BarToBoardMoveMutation MutationData)
        {
            OnBarToBoardMove?.Invoke(MutationData);
        }

        public static void OnBoardToBarCall(BoardToBarMoveMutation MutationData)
        {
            OnBoardToBarMove?.Invoke(MutationData);
        }

        public static void OnBoardToBoardCall(BoardToBoardMoveMutation MutationData)
        {
            OnBoardToBoardMove?.Invoke(MutationData);
        }

        public static void OnDiceChangedCall(DiceChangedMutation MutationData)
        {
            OnDiceChanged?.Invoke(MutationData);
        }

    }
}
