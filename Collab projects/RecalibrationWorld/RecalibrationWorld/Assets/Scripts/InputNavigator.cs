using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputNavigator : MonoBehaviour
{
    EventSystem system;

    void Start()
    {
        system = EventSystem.current;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {
                InputField inputfield = next.GetComponent<InputField>();
                Dropdown dropDown = next.GetComponent<Dropdown>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));
                else if(dropDown != null)
                {
                    dropDown.OnPointerClick(new PointerEventData(system));
                }

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }

            //Here is the navigating back part:
            else
            {
                next = Selectable.allSelectables[0];
                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }

        }
    }
}