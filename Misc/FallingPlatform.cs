using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour, IEntityContact
{
    public bool autoReset = true;
    public float fallDelay = 2f;
    public float resetDelay = 5f;
    public float fallGravity = 40f;

    [Header("Shake Setting")]
    public bool shake = true;
    public float speed = 45f;
    public float height = 0.1f;

    protected Collider m_collider;
    protected Vector3 m_initialPosition;

    protected Collider[] m_overlaps = new Collider[32];

    public bool activated { get; protected set; }

    public bool falling { get; protected set; }

    public virtual void Fall()
    {
        falling = true;
        m_collider.isTrigger = true;
    }

    public virtual void Restart()
    {
        activated = falling = false;
        transform.position = m_initialPosition;
        m_collider.isTrigger = false;
        OffsetPlayer();
    }

    public void OnEntityContact(Entity entity)
    {
        if (entity is Player && entity.IsPointUnderStep(m_collider.bounds.max))
        {
            if (!activated)
            {
                activated = true;
                StartCoroutine(Routine());
            }
        }
    }

    protected virtual void OffsetPlayer()
    {
        var center = m_collider.bounds.center;
        var extents = m_collider.bounds.extents;
        var maxY = m_collider.bounds.max.y;
        var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);

        for (int i = 0; i < overlaps; i++)
        {
            if (!m_overlaps[i].CompareTag(GameTags.Player))
                continue;

            var distance = maxY - m_overlaps[i].transform.position.y;
            var height = m_overlaps[i].GetComponent<Player>().height;
            var offset = Vector3.up * (distance + height * 0.5f);

            m_overlaps[i].transform.position += offset;
        }
    }

    protected IEnumerator Routine()
    {
        var timer = fallDelay;

        while (timer >= 0)
        {
            if (shake && (timer <= fallDelay / 2f))
            {
                var shake = Mathf.Sin(Time.time * speed) * height;
                transform.position = m_initialPosition + Vector3.up * shake;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        Fall();

        if (autoReset)
        {
            yield return new WaitForSeconds(resetDelay);
            Restart();
        }
    }

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_initialPosition = transform.position;
        tag = GameTags.Platform;
    }

    protected virtual void Update()
    {
        if (falling)
        {
            transform.position += fallGravity * Vector3.down * Time.deltaTime;
        }
    }
}
