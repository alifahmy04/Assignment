using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace ElementalAnomaly.Prototype
{
    public sealed class PrototypeReactorCinematic : MonoBehaviour
    {
        public PrototypePlayerController player;
        public PrototypeCinemachineExploration explorationInput;
        public CinemachineCamera reactorCamera;
        public CinemachineCamera exitCamera;

        [Header("Shot Timing")]
        [Min(0f)] public float reactorBlendSeconds = 0.5f;
        [Min(0f)] public float reactorHoldSeconds = 1.4f;
        [Min(0f)] public float exitBlendSeconds = 0.5f;
        [Min(0f)] public float exitHoldSeconds = 2.1f;
        [Min(0f)] public float returnBlendSeconds = 0.6f;

        public bool IsPlaying { get; private set; }
        private bool played, previousInputLock, previousCinematicState;
        private int previousReactorPriority, previousExitPriority;
        private Action revealReactor, revealExit;

        public bool TryPlay(Action onReactorReveal, Action onExitReveal)
        {
            if (!isActiveAndEnabled || played || player == null || explorationInput == null || reactorCamera == null || exitCamera == null)
                return false;
            if (Camera.main == null || Camera.main.GetComponent<CinemachineBrain>() == null) return false;

            played = true;
            IsPlaying = true;
            revealReactor = onReactorReveal;
            revealExit = onExitReveal;
            previousInputLock = player.InputLocked;
            previousCinematicState = explorationInput.CinematicActive;
            previousReactorPriority = reactorCamera.Priority;
            previousExitPriority = exitCamera.Priority;
            player.SetInputLocked(true);
            explorationInput.SetCinematicActive(true);
            reactorCamera.Priority = 100;
            StartCoroutine(Sequence());
            return true;
        }

        private IEnumerator Sequence()
        {
            try
            {
                yield return new WaitForSecondsRealtime(reactorBlendSeconds);
                RevealReactor();
                yield return new WaitForSecondsRealtime(reactorHoldSeconds);
                exitCamera.Priority = 110;
                yield return new WaitForSecondsRealtime(exitBlendSeconds);
                RevealExit();
                yield return new WaitForSecondsRealtime(exitHoldSeconds);
                reactorCamera.Priority = previousReactorPriority;
                exitCamera.Priority = previousExitPriority;
                yield return new WaitForSecondsRealtime(returnBlendSeconds);
            }
            finally
            {
                CompleteAndRestore();
            }
        }

        private void RevealReactor()
        {
            var action = revealReactor;
            revealReactor = null;
            action?.Invoke();
        }

        private void RevealExit()
        {
            var action = revealExit;
            revealExit = null;
            action?.Invoke();
        }

        private void CompleteAndRestore()
        {
            if (!IsPlaying) return;
            try
            {
                RevealReactor();
                RevealExit();
            }
            finally
            {
                if (reactorCamera != null) reactorCamera.Priority = previousReactorPriority;
                if (exitCamera != null) exitCamera.Priority = previousExitPriority;
                if (explorationInput != null) explorationInput.SetCinematicActive(previousCinematicState);
                if (player != null) player.SetInputLocked(previousInputLock);
                IsPlaying = false;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            CompleteAndRestore();
        }
    }
}
