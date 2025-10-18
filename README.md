# ES2MD: ExplorerScript to Markdown

## Introduction

This desktop app is designed to transpile SkyTemple ExplorerScript cutscene files into visual Markdown for ease of reading and review, and for use in related projects.

Many thanks to Miles Farber for providing the inspiration and the impetus for the creation of this program.


## Usage

In the ES2MD window, press the "Select..." button to choose one more EXPS files to transpile to Markdown. As ES2MD proceeds with transpiling, the window will populate with metrics of each unique ExplorerScript object type. This info is useful for debugging purposes.

ES2MD will create two subfolders in the local directory, labeled "Syntax Trees/" and "Markdown/".

"Syntax Trees/" contains verbose interpretations of the selected EXPS files in a raw plaintext format. Like the object metrics, this info is useful for debugging.

"Markdown/" contains the final output markdown files, separated into further subfolders sharing the name of the related ExplorerScript file. Each .md file in a subfolder corresponds to a single cutscene within an ExplorerScript file, ordered the same as they were in that file.
