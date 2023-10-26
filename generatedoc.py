import os
import re

# 指定源文件夹和目标文件夹
source_folder = 'your_source_folder'  # 替换为实际的源文件夹路径
output_folder = 'your_output_folder'  # 替换为实际的目标文件夹路径

# 遍历源文件夹下的所有.cs文件
for root, dirs, files in os.walk(source_folder):
    for file in files:
        if file.endswith(".cs"):
            file_path = os.path.join(root, file)
            
            # 读取.cs文件内容
            with open(file_path, 'r') as cs_file:
                csharp_code = cs_file.read()

                # 使用正则表达式提取属性和方法的注释
                properties = re.findall(r'///\s*<summary>\n(.*?)\n\s*///\s*<param name="(.*?)">(.*?)</param>', csharp_code, re.DOTALL)
                methods = re.findall(r'///\s*<summary>\n(.*?)\n\s*///\s*<param name="(.*?)">(.*?)</param>', csharp_code, re.DOTALL)

                # 创建相应的.md文件并写入内容
                md_file_path = os.path.join(output_folder, os.path.splitext(file)[0] + ".md")

                with open(md_file_path, 'w') as md_file:
                    # 写入Markdown标题
                    md_file.write(f"# {os.path.splitext(file)[0]}\n\n")

                    # 处理属性
                    md_file.write("## Properties\n\n| Property | Type | Description |\n| :--- | :--- | :--- |\n")
                    for property_info in properties:
                        description, _, property_name, property_type = property_info
                        md_file.write(f"| {property_name} | [{property_type}]({property_type}) | {description} |\n")

                    # 处理方法
                    md_file.write("\n## API\n\n| Function | Return Type | Description | Parameters |\n :-- | :-- | :-- | :-- |\n")
                    for method_info in methods:
                        description, _, _, method_name, _, parameters = method_info
                        md_file.write(f"| {method_name} | void | {description} | {parameters} |\n")
