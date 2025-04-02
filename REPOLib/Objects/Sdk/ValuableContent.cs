﻿using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib ValuableContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private ValuableObject _prefab = null!;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    [SerializeField]
    private string[] _valuablePresets = [Modules.ValuablePresets.GenericValuablePresetName];

    /// <summary>
    /// The <see cref="ValuableObject"/> of this content.
    /// </summary>
    public ValuableObject Prefab => _prefab;

    /// <summary>
    /// The list of valuable presets for this content.
    /// </summary>
    public IReadOnlyList<string> ValuablePresets => _valuablePresets;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => Prefab.name;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Valuables.RegisterValuable(Prefab.gameObject, ValuablePresets.ToList());
    }
}
