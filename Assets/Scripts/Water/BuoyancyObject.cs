using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class BuoyancyObject : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Floating Points")]
    public List<Transform> _floatingPoint = new();
    [HideInInspector] public int floatingPointsUnderwater;

    [Header("Drag Coefficients")]
    [SerializeField] private float _underwaterDrag = 3f;
    [SerializeField] private float _underwaterAngularDrag = 1f;
    [SerializeField] private float _airDrag = 0f;
    [SerializeField] private float _airAngularDrag = 0.05f;

    [Header("Float Effects")]
    [Range(1.0f, 500.0f)]
    public float _floatingPower = 15f;
    [SerializeField] private float _waterHeight = 0;

    private bool isUnderwater;

    [Header("Gizmos")]
    [SerializeField] private bool gizmosActive;
    [Range(0.1f, 2.0f)]
    [SerializeField] private float gizmosSize = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!_floatingPoint.Any())
        {
            _floatingPoint.Add(transform);
        }
    }

    /*    private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Water"))
                _waterHeight = other.gameObject.transform.position.y;
        }*/


    void FixedUpdate()
    {
        floatingPointsUnderwater = 0;

        for (int i = 0; i < _floatingPoint.Count; i++)
        { 
            float diff = Mathf.Clamp(_floatingPoint[i].position.y - _waterHeight, -1f, 1f);

            if (diff < 0)   //checks if floating point is below waterline
            {
                rb.AddForceAtPosition(Vector3.up * _floatingPower * Mathf.Abs(diff), _floatingPoint[i].transform.position, ForceMode.Force);

                floatingPointsUnderwater++;

                if (!isUnderwater)
                {
                    isUnderwater = true;
                    SwitchState(true);
                }
            }
        }

        if (isUnderwater && floatingPointsUnderwater == 0)
        {
            isUnderwater = false;
            SwitchState(false);
        }
            
    }

    void SwitchState(bool Underwater)   //switches phyiscs values depending on if object is in water
    {
        if (Underwater)
        {
            rb.drag = _underwaterDrag;
            rb.angularDrag = _underwaterAngularDrag;
        }else
        {
            rb.drag = _airDrag;
            rb.angularDrag = _airAngularDrag;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (gizmosActive)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < _floatingPoint.Count; i++)
            {
                Gizmos.DrawWireSphere(_floatingPoint[i].position, gizmosSize);
            }
        }
    }
}
