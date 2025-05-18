import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
VIRT_PATH = os.path.join(SCRIPT_DIR, "Assets", "CybSDK", "Core", "Scripts", "CVirtPlayerController.cs")

def patch_virtualizer_orientation_fields(lines):
    """
    Patches the CVirtPlayerController.cs script by:
    1. Inserting a GlobalOrientation field after MotionVector
    2. Inserting an assignment for GlobalOrientation before motionVector = ...
    Skips patching if lines already exist.
    """
    motion_vector_declaration_marker = "public Vector3 MotionVector { get; private set; }"
    field_comment = "    // Added in order to be able to extract the player rotation angle"
    field_declaration = "    public Quaternion GlobalOrientation { get; private set; }"

    global_assignment_line = "        GlobalOrientation = globalOrientation;"
    motion_vector_assignment_marker = "Vector3 motionVector = globalOrientation * movement;"

    already_has_field = any(field_declaration in line for line in lines)
    already_has_assignment = any(global_assignment_line.strip() == line.strip() for line in lines)

    new_lines = []
    field_inserted = False
    assignment_inserted = False

    for i, line in enumerate(lines):
        new_lines.append(line)
        if not already_has_field and motion_vector_declaration_marker in line and not field_inserted:
            new_lines.append("\n")
            new_lines.append(field_comment + "\n")
            new_lines.append(field_declaration + "\n")
            field_inserted = True

    final_lines = []
    for line in new_lines:
        if not already_has_assignment and motion_vector_assignment_marker in line and not assignment_inserted:
            final_lines.append(global_assignment_line + "\n")
            assignment_inserted = True
        final_lines.append(line)

    return final_lines, field_inserted or already_has_field, assignment_inserted or already_has_assignment

def apply_patch_to_file(path, patch_function):
    if not os.path.exists(path):
        print(f"Error: File not found at {path}")
        return False

    try:
        with open(path, "r", encoding="utf-8") as f:
            original_lines = f.readlines()
    except UnicodeDecodeError:
        with open(path, "r", encoding="cp1252") as f:
            original_lines = f.readlines()

    patched_lines, field_ok, assign_ok = patch_function(original_lines)

    if field_ok and assign_ok:
        with open(path, "w", encoding="cp1252") as f:
            f.writelines(patched_lines)
        print(f"Patched: {os.path.basename(path)}")
    else:
        print(f"Skipped: {os.path.basename(path)} already patched")

    return field_ok and assign_ok

def main():
    print("Running Universal Patcher")
    tasks = [
        ("Patching CVirtPlayerController...", VIRT_PATH, patch_virtualizer_orientation_fields),
    ]

    for label, path, func in tasks:
        print("\n" + label)
        apply_patch_to_file(path, func)

if __name__ == "__main__":
    main()
