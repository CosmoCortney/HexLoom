# HexLoom
HexLoom is a simple yet powerful tool for editing binary data. Unlike traditional hex editors, HexLoom introduces a new concept: instead of directly modifying binary data, users define all edits in a customizable list. These changes are applied only once finalized. A dual-hex-view allows users to compare the original data (on the left) with the modified version (on the right), making it easy to trace and correct errors.

It also supports cmd-mode so multiple project files can be processed via bash scripts!

![image](https://github.com/user-attachments/assets/9cf26594-32da-48b0-a665-cfc40ee4e8ad)


## Features
Customizable Table-Based Editing: Define all changes in a table before applying.
<br>
Dual-Hex-View: Compare original and modified data side by side for easy error tracking.
<br>
Wide Range of Supported Data Types:
- Primitives:
  - Int8, UInt8, Int16, UInt16, Int32, UInt32, Int64, UInt64
  - Float32, Float64
  - Bool
- Arrays (any dimension):
  - Int8, UInt8, Int16, UInt16, Int32, UInt32, Int64, UInt64, Float32, Float64
- Colors:
  - RGB, RGBA, RGBF, RGBAF
- Strings:
  - UTF-8, UTF-16 (Little Endian, Big Endian), UTF-32 (Little Endian, Big Endian)
  - ASCII
  - ISO-8859-1 to ISO-8859-16
  - Shift JIS Code Page 932
  - JIS X 0201 (Full Width Katakana, Half Width Katakana)
  - KS X 1001
  - Pokémon Gen I (English, French & German, Italian & Spanish, Japanese)
  - Pokémon Gen II English
- CMD Mode

## Usage
### Creating a new project
1. click file, new project
2. enter all properties that apply to your project and source file
3. click ok

### Adding Groups and Entities
1. click `Add Group` to add your first group
2. give the group a name
3. click `Add Entity` to add your first entity
4. check whether you want it to be applied. Leave it unchecked to let it be ignored
5. give the entity a name
6. add its address (hex)
7. select its primary datatype
8. select a secondary datatype
9. enter your value
    - Primitives
      - Integers: Either hex or decimal
      - Booleans: check the checkbox if the value is supposed to be true
      - Floats/Doubles: use a dot as a decimal separator
    - Arrays: Values are written inside square brackets separated by commas (e.g.: `[0x1337, 69, 420]`) for multi-dimensional arrays put each dimension in square brackets as well (e.g. `[[0x1337, 69, 420], [0x1337, 69, 420]]`). There is no limit on dimensions.
    - Colors
      - RGB(A): use standard RGB(A) notations (e.g. RGB: `#8000FF`, RGBA: `#8000FFFF`)
      - RGBF(A): write an array of floats representing the colos. (e.g. RGBF: `[0.5, 0.0, 1.0]`, RGBFA: `[0.5, 0.0, 1.0, 1.0]`
    - Strings: Just type in your text. It will become converted to the selected encoding (secondary type)

### Applying  Changes
1. click `Apply`

### Export edited file
1. Go to `file`, `Generate Output File`

### Open an existing project
1. go to `file`, `Open Project`

### View your edits
1. Enter the desired address (hex) in the hex editor view
2. click on the first byte of the value to view it in different formats below
3. select the desired character encoding (note that your string might look off for multi-byte characters on line breaks. But the binary data is correct)

### Cmd mode
1. create a .bat file calling HexLoom.exe
2. add "--cmd" as an argument. The second argument must be a path to a HexLoom project file (ends in .json). I recommend using relative paths if you wanna share your .bat file.
3. HexLoom will now run in the background and export your edited file

