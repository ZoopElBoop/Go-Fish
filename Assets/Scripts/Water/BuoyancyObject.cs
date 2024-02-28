using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class BuoyancyObject : MonoBehaviour
{
    [Header("Floating Points")]
    [SerializeField] private List<Transform> _floatingPoint = new();
    private int floatingPointsUnderwater;

    [Header("Drag Coefficients")]
    [SerializeField] private float _underwaterDrag = 3f;
    [SerializeField] private float _underwaterAngularDrag = 1f;
    [SerializeField] private float _airDrag = 0f;
    [SerializeField] private float _airAngularDrag = 0.05f;

    [Header("Float Effects")]
    [Range(1.0f, 100.0f)]
    public float _floatingPower = 15f;
    [SerializeField] private float _waterHeight = -100f;

    [SerializeField] Rigidbody rb;

    private bool isUnderwater;

    [Header("Gizmos")]
    [SerializeField] private bool gizmosActive;
    [Range(0.1f, 2.0f)]
    [SerializeField] private float gizmosSize = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!_floatingPoint.Any())
        {
            _floatingPoint.Add(transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
            _waterHeight = other.gameObject.transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        floatingPointsUnderwater = 0;

        for (int i = 0; i < _floatingPoint.Count; i++)
        { 
            float diff = _floatingPoint[i].position.y - _waterHeight;

            if (diff < 0)
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

    void SwitchState(bool Underwater) 
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
