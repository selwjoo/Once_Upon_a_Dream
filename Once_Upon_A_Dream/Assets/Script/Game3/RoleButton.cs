using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoleButton : MonoBehaviour
{

    public string role;
    public RoleManager roleManager;

    public void OnRoleButtonClick()
    {
        // --- 1. 서버에 역할 선택 요청 ---
        roleManager.ChooseRole(role);

        // --- 2. GameManager에 로컬 저장 ---
        GameManager.Instance.chosenRole = role;

        // --- 3. 역할 체크 ---
        CheckBothRolesSelected();
    }

    public void CheckBothRolesSelected()
    {
        var roles = GameManager.Instance.playerRoles;

        bool hasRoleA = roles.ContainsValue("RoleA");
        bool hasRoleB = roles.ContainsValue("RoleB");

        if (hasRoleA && hasRoleB)
        {
            Debug.Log("두 역할 모두 선택됨 → 게임 시작");
            SceneManager.LoadScene("Game3");
        }
    }
}
