from typing import List


def create_template(source_file: str, template_file: str):
    print("Creating template:")
    print(source_file)
    print(" -->", template_file)
    
    lines = open(source_file).read().splitlines()
    
    lines = remove_marked_lines(lines)
    lines = mark_namespace(lines)
    
    with open(template_file, "w") as f:
        for line in lines:
            f.write(line + "\n")
            
            
def mark_namespace(lines: List[str]):
    namespace_found = False
    
    i = 0
    while i < len(lines):
        if lines[i].startswith("namespace"):
            lines[i] = "$NAMESPACE_BEGIN$"
            if lines[i + 1] != "{":
                raise Exception("Namespace lacks opening brace.")
            del lines[i + 1]
            namespace_found = True
            break
        i += 1
    
    if not namespace_found:
        raise Exception("No namespace found.")
        
    while lines[-1] == "":
        del lines[-1]
        
    if lines[-1] != "}":
        raise Exception("Last line is not a closing brace.")
        
    lines[-1] = "$NAMESPACE_END$\n"

    return lines
    
    
def remove_marked_lines(lines: List[str]):
    i = 0
    while i < len(lines):
        if lines[i].endswith("// $$ REMOVE_FROM_TEMPLATE"):
            del lines[i]
            i -= 1
        i += 1
       
    return lines


########
# Main #
########

fixture_path = "../Assets/UnisaveFixture/"
templates_path = "../Assets/Unisave/Templates/"

# Email authentication
print("\nEMAIL AUTHENTICATION")
create_template(
    fixture_path + "Backend/EmailAuthentication/EmailAuthUtils.cs",
    templates_path + "EmailAuthentication/EmailAuthUtils.txt"
)
create_template(
    fixture_path + "Backend/EmailAuthentication/EmailLoginFacet.cs",
    templates_path + "EmailAuthentication/EmailLoginFacet.txt"
)
create_template(
    fixture_path + "Backend/EmailAuthentication/EmailRegisterFacet.cs",
    templates_path + "EmailAuthentication/EmailRegisterFacet.txt"
)
create_template(
    fixture_path + "Backend/EmailAuthentication/EmailRegisterResponse.cs",
    templates_path + "EmailAuthentication/EmailRegisterResponse.txt"
)
create_template(
    fixture_path + "Scripts/EmailAuthentication/EmailLoginForm.cs",
    templates_path + "EmailAuthentication/EmailLoginForm.txt"
)
create_template(
    fixture_path + "Scripts/EmailAuthentication/EmailRegisterForm.cs",
    templates_path + "EmailAuthentication/EmailRegisterForm.txt"
)

# Steam authentication
print("\nSTEAM AUTHENTICATION")
create_template(
    fixture_path + "Backend/SteamAuthentication/SteamLoginFacet.cs",
    templates_path + "SteamAuthentication/SteamLoginFacet.txt"
)
create_template(
    fixture_path + "Scripts/SteamAuthentication/SteamLoginClient.cs",
    templates_path + "SteamAuthentication/SteamLoginClient.txt"
)

# Steam microtransactions
print("\nSTEAM MICROTRANSACTIONS")
create_template(
    fixture_path + "Backend/SteamMicrotransactions/VirtualProducts/ExampleVirtualProduct.cs",
    templates_path + "SteamMicrotransactions/ExampleVirtualProduct.txt"
)
create_template(
    fixture_path + "Backend/SteamMicrotransactions/IVirtualProduct.cs",
    templates_path + "SteamMicrotransactions/IVirtualProduct.txt"
)
create_template(
    fixture_path + "Backend/SteamMicrotransactions/SteamPurchasingServerFacet.cs",
    templates_path + "SteamMicrotransactions/SteamPurchasingServerFacet.txt"
)
create_template(
    fixture_path + "Backend/SteamMicrotransactions/SteamTransactionEntity.cs",
    templates_path + "SteamMicrotransactions/SteamTransactionEntity.txt"
)
create_template(
    fixture_path + "Scripts/SteamMicrotransactions/SteamPurchasingClient.cs",
    templates_path + "SteamMicrotransactions/SteamPurchasingClient.txt"
)
