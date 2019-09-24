using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Utilities
{
    public class RatioFixer : MonoBehaviour
    {


        void Start()
        {
            FitToScreen();
        }

        /// <summary>
        /// Resizes background to fit the screen.
        /// </summary>
        protected void FitToScreen()
        {
            // Centering object in the world
            transform.localPosition = Vector3.zero;

            // Scaling object
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            Vector3 newScale = new Vector3(1, 1, 1);

            float spriteWidth = renderer.sprite.bounds.size.x;
            float spriteHeight = renderer.sprite.bounds.size.y;

            float screenHeight = Camera.main.orthographicSize * 2.0f;
            float screenWidth = screenHeight / Screen.height * Screen.width;

            newScale.x = screenWidth / spriteWidth;
            newScale.y = screenHeight / spriteHeight;

            transform.localScale = newScale;

        }
    }
}