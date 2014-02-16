using UnityEngine;
using System.Collections;

[System.Serializable]
public class LayerId
{
	public int value;
	
	public static implicit operator int(LayerId source)
	{
		return source.value;
	}
	
	public static implicit operator LayerId(int source)
	{
		return new LayerId() { value = source };
	}
}
