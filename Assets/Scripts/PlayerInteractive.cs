﻿using UnityEngine;

namespace PlayerClasses
{
    public sealed class PlayerInteractive : MonoBehaviour
    {
        private void Start()
        {
            mainCamera = Camera.main;
            playerStatements = GetComponent<PlayerStatements>();
        }

        Camera mainCamera;
        float interctionDistance = 2;
        [SerializeField] LayerMask interactionLayer;
        private KeyCode inputInteractive = KeyCode.F;
        private PlayerStatements playerStatements;
        private Vector3 rayStartPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        private bool inputedButton = false;

        private void Update()
        {
            if (Input.GetKeyDown(inputInteractive))
            {
                inputedButton = true;              
            }
        }
        private void FixedUpdate()
        {
            RayThrow();
        }
        private void RayThrow()
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(rayStartPos);
            string desc = string.Empty;
            if (Physics.Raycast(ray, out hit, interctionDistance, interactionLayer))
            {
                if (hit.transform.TryGetComponent<InteractiveObject>(out var component))
                {
                    desc = component.GetDescription();
                    if (inputedButton)
                    {
                        component.Interact(playerStatements);
                        inputedButton = false;
                    }
                }
            }
            DescriptionDrawer.Instance.SetHint(desc);
        }
    }    
}