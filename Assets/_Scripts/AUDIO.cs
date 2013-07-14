using UnityEngine;
using System.Collections;

public class AUDIO : MonoBehaviour {

	public AudioClip hitSound;
	public AudioClip explosionSound;
	public AudioClip collectSound;
	public AudioClip shootSound;

	public AudioClip[] killSound;

	public void PlayRandomKillSound()
	{
		int rand = Random.Range(0, killSound.Length-1);
		AudioSource.PlayClipAtPoint(killSound[rand], transform.position);
	}
}
