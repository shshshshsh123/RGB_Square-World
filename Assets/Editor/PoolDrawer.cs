using UnityEditor; // 에디터 스크립트를 위해 필요
using UnityEngine;

// ObjectPooler 클래스 내부에 정의된 Pool 클래스에 대한 PropertyDrawer임을 명시
[CustomPropertyDrawer(typeof(ObjectPooler.Pool))]
public class PoolDrawer : PropertyDrawer
{
    // Inspector에서 각 Pool 항목을 어떻게 그릴지 정의하는 함수
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // PropertyScope를 시작하여 올바른 컨텍스트에서 작업하도록 함
        EditorGUI.BeginProperty(position, label, property);

        // Pool 클래스 내부의 'type' 프로퍼티를 찾음 (enum 타입)
        SerializedProperty typeProperty = property.FindPropertyRelative("type");
        // 'initalSize' 프로퍼티를 찾음 (정수 타입)
        SerializedProperty sizeProperty = property.FindPropertyRelative("initalSize");

        // 현재 선택된 enum 값의 이름을 가져옴
        string enumName = typeProperty.enumDisplayNames[typeProperty.enumValueIndex];
        string headerLabelText = $"{enumName} (Size: {sizeProperty.intValue})";

        // 기본 "Element n" 라벨 대신 이름을 사용한 새로운 라벨 생성
        GUIContent poolLabel = new GUIContent(headerLabelText);

        // 접혔다 펴지는 Foldout 헤더를 그림
        // property.isExpanded는 현재 항목이 펼쳐져 있는지 여부를 저장
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, poolLabel, true);

        // 만약 Foldout이 펼쳐져 있다면, 내부 프로퍼티들을 그림
        if (property.isExpanded)
        {
            // 들여쓰기 시작
            EditorGUI.indentLevel++;

            // 각 프로퍼티('type', 'prefab', 'initalSize')를 그릴 Rect 계산
            Rect typeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
            Rect prefabRect = new Rect(position.x, typeRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
            Rect sizeRect = new Rect(position.x, prefabRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

            // 각 프로퍼티 필드를 그림
            EditorGUI.PropertyField(typeRect, typeProperty); // type enum 선택 필드
            EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("prefab")); // prefab 할당 필드
            EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("initalSize")); // initalSize 정수 필드

            // 들여쓰기 끝
            EditorGUI.indentLevel--;
        }

        // PropertyScope 종료
        EditorGUI.EndProperty();
    }

    // Foldout 상태에 따라 Property의 높이를 동적으로 계산하는 함수
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight; // 기본 Foldout 헤더 높이

        // 만약 펼쳐져 있다면, 각 필드의 높이와 간격을 더함
        if (property.isExpanded)
        {
            totalHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3; // 3개의 필드
        }

        return totalHeight;
    }
}