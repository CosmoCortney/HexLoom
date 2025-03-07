# HexLoom
HexLoom is a simple yet powerful tool for editing binary data. Unlike traditional hex editors, HexLoom introduces a new concept: instead of directly modifying binary data, users define all edits in a customizable table. These changes are applied only once finalized. A dual-hex-view allows users to compare the original data (on the left) with the modified version (on the right), making it easy to trace and correct errors.
<br><br>
<b>Note:</b> The project is still WIP, and no release build is available yet.

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