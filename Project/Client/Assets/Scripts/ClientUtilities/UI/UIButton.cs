
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using System.Collections;
using UnityEngine;

namespace ClientUtilities.UI
{
    public class UIButton : UnityEngine.UI.Button
    {
        private Vector3 ScaleSize;
        private Vector3 OrginalSize;
        private float effectInterval = 50F;
        private bool isProcess;

        protected override void Start()
        {
            OrginalSize = this.transform.localScale;
            ScaleSize = this.transform.localScale * 0.95F;

            this.onClick.AddListener(OnClickEffect);
        }

        private void OnClickEffect()
        {
            if (isProcess || !gameObject.activeSelf || !gameObject.activeInHierarchy)
                return;
           
            StartCoroutine(DoEffect());
        }

        private IEnumerator DoEffect()
        {
            if(!gameObject.activeSelf || !gameObject.activeInHierarchy)
            {
                CancelInvoke("DoEffect");
              
                this.transform.localScale = OrginalSize;
                isProcess = false;
                yield return null;
            }
            isProcess = true;
            //To Do May be I Should use LeanTwean Instead of Coroutine
            while (this.transform.localScale.x > ScaleSize.x)
            {
                this.transform.localScale = Vector3.Lerp(this.transform.localScale, ScaleSize, effectInterval * Time.deltaTime);
                yield return null;
            }
            this.transform.localScale = ScaleSize;
            while (this.transform.localScale.x < OrginalSize.x)
            {
                this.transform.localScale = Vector3.Lerp(this.transform.localScale, OrginalSize, effectInterval * Time.deltaTime);
                yield return null;
            }
            this.transform.localScale = OrginalSize;
            isProcess = false;
            yield return new WaitForSeconds(0);

        }
    }
}