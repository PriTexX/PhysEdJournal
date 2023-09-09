import os

def replaceLinksInFiles():
    for root, _, fileNames in os.walk("docs"):
        for fileName in fileNames:
            if fileName.split('.')[-1] != 'md':
                continue

            with open(f"{root}/{fileName}", 'r') as file:
                content = file.read()

            content = content.replace("/.eraser/", "/test-doc/img/")

            with open(f"{root}/{fileName}", 'w') as file:
                file.write(content)

replaceLinksInFiles()
if not os.path.exists("docs/img"):
    os.mkdir("docs/img")
