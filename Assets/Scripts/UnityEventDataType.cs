using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityEventFloat : UnityEvent<float> { }

[Serializable]
public class UnityEventInt : UnityEvent<int> { }

[Serializable]
public class UnityEventVector3 : UnityEvent<Vector3> { }

[Serializable]
public class UnityEventVector2 : UnityEvent<Vector2> { }

[Serializable]
public class UnityEventClass<T> : UnityEvent<T> where T : MonoBehaviour { }