using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLevelControl : MonoBehaviour {
    // Structs for EnvironmentState, PlayerState, EnemyState

    // Save points, Modified objects, Goal
    EnvironmentState EnvironmentStates[];
    // Health, Stamina, Inventory, State (onGround, Climbing, onSloop, ...)
    PlayerState PlayerStates[];
    // ID, Health, Points, DropRate[HowMuch, HowRare], isPlayer, speed
    EnemyState EnemyStates[];

    // Standard start function
    void Start()
    {
        // Level file in unmodifiable

        // Load from level file
        LoadEnvironment(EnvironmentStates);
        // Load from level file
        LoadEnemy(EnemyStates);
        // Load from state file
        LoadPlayer(PlayerStates);
    }

    // Standard main loop
    void Update () {
        UpdateCharacterState();
        UpdateEnemyState();
        UpdateEnvironmentState();
    }

    // Standard controller
    private void FixedUpdate()
    {
        PlayerController.Move(CharacterStates);
        EnemyController.Move(EnemyStates);
        EnvironmentController.Update(EnvironmentStates);
    }
}
