# CoaSTL
CoaSTL creates custom coaster STL files, with optimisation for 3D printing.

# 3D Printable Coaster Designer - Requirements Specification

## Project Overview
A .NET desktop application for designing custom 3D-printable coasters with image integration, optimized for Bambu Labs printers and generating STL files compatible with Bambu Studio/Bambu Handy.

---

## 1. Core Functional Requirements

### 1.1 Image Input & Processing
- **Image Import**: Support common formats (PNG, JPG, JPEG, SVG, BMP, TIFF)
- **Image Editing Tools**:
  - Crop, rotate, scale, and position adjustments
  - Brightness, contrast, and threshold controls
  - Convert to grayscale/monochrome for relief mapping
  - Edge detection and outline extraction
  - Background removal capability
- **Image-to-3D Conversion**:
  - Height map generation from image brightness
  - Adjustable relief depth (0.5mm - 5mm)
  - Invert relief option (embossed vs debossed)
  - Smoothing and detail preservation controls

### 1.2 Coaster Shapes & Styles
- **Standard Shapes**:
  - Circle (various diameters: 90mm, 100mm, 110mm)
  - Square (80mm, 90mm, 100mm)
  - Hexagon
  - Octagon
  - Rounded square with adjustable corner radius
  - Custom polygon (3-12 sides)
- **Edge Styles**:
  - Flat edge
  - Beveled edge (adjustable angle)
  - Rounded edge
  - Decorative borders (scalloped, geometric patterns)
  - Raised rim for containment
- **Base Patterns**:
  - Solid base
  - Honeycomb infill (visible bottom pattern)
  - Lattice/grid patterns
  - Concentric circles
  - Custom drainage channels

### 1.3 3D Design Features
- **Surface Treatments**:
  - Embossed text (customizable font, size, depth)
  - Debossed text
  - Logo/icon placement with depth control
  - Multi-layer designs (combine multiple images at different heights)
  - Voronoi/organic patterns
  - Geometric 3D patterns (pyramids, waves, bubbles)
- **Structural Elements**:
  - Non-slip bottom patterns (dots, lines, grid)
  - Cork/rubber insert recesses
  - Stacking features (nubs and recesses for storage)
  - Drainage grooves on top surface
  - Air gap features to prevent suction
- **Advanced 3D Options**:
  - Multi-material regions (for MMU/AMS printing)
  - Variable thickness across surface
  - Topographic/contour line effects
  - Lithophane mode (thin translucent areas for backlit display)
  - Spiral/twist effects

### 1.4 Customization Parameters
- **Dimensions**:
  - Diameter/width: 70mm - 150mm (adjustable in 1mm increments)
  - Base thickness: 2mm - 8mm
  - Total height: 3mm - 15mm
  - Rim height (if applicable): 1mm - 5mm
- **Material Settings**:
  - Estimated filament usage calculation
  - Material presets (PLA, PETG, TPU, Wood PLA, etc.)
  - Infill density recommendation
  - Layer height suggestions
- **Design Templates**:
  - Pre-made templates (minimalist, ornate, modern, vintage)
  - Template library with preview
  - Save/load custom templates
  - Import community templates (JSON format)

---

## 2. Bambu Labs Integration

### 2.1 Bambu Studio Optimization
- **Printer Profiles**:
  - Pre-configured profiles for all Bambu Labs models:
    - X1 Carbon
    - X1E
    - P1P
    - P1S
    - A1 series
    - A1 mini
- **Optimized Export Settings**:
  - Recommended layer heights (0.2mm standard, 0.12mm detailed)
  - Optimal print speeds for coasters
  - Support-free designs where possible
  - First layer adhesion optimization
- **Multi-Material Support**:
  - AMS unit integration for multi-color designs
  - Filament change planning for color transitions
  - Purge tower optimization for small prints
  - Color assignment interface for different design regions

### 2.2 STL File Generation
- **High-Quality Mesh**:
  - Manifold geometry (watertight meshes)
  - Optimized polygon count (balance detail vs file size)
  - Normal consistency checking
  - Self-intersection detection and repair
- **Export Options**:
  - Binary or ASCII STL format
  - Unit specification (millimeters)
  - Coordinate system orientation (Z-up for 3D printing)
  - Mesh resolution settings (draft, standard, high, ultra)
- **Batch Export**:
  - Export multiple designs simultaneously
  - Plate arrangement for batch printing
  - Naming conventions with auto-numbering

### 2.3 Bambu Handy Integration
- **Mobile Workflow**:
  - QR code generation for quick mobile transfer
  - Cloud upload capability for Bambu Cloud
  - Optimized preview thumbnails embedded in STL
  - Print time and material estimates in metadata

---

## 3. User Interface Requirements

### 3.1 Main Application Window
- **Design Canvas**:
  - 3D viewport with orbit/pan/zoom controls
  - Orthographic and perspective view modes
  - Real-time preview rendering
  - Grid and measurement overlays
  - Lighting controls for preview accuracy
- **Toolbar**:
  - Quick access to common shapes and tools
  - Undo/redo functionality (20+ step history)
  - Alignment and distribution tools
  - Measurement tools
- **Property Panels**:
  - Hierarchical design tree
  - Parameter sliders with numeric input
  - Color pickers for multi-material designs
  - Real-time parameter preview

### 3.2 Workflow
- **Step-by-Step Wizard** (optional for beginners):
  1. Choose base shape and size
  2. Import/design surface pattern
  3. Add 3D features
  4. Configure bottom features
  5. Review and export
- **Advanced Mode**:
  - Direct access to all features
  - Multi-tab workspace for complex designs
  - Component library panel
  - Layer management system

### 3.3 Accessibility
- **Responsive Design**:
  - Minimum resolution: 1280x720
  - Scalable UI elements
  - Dark and light themes
  - High-contrast mode option
- **Usability**:
  - Keyboard shortcuts for common operations
  - Context-sensitive help tooltips
  - Tutorial videos/documentation access
  - Sample projects for learning

---

## 4. Technical Architecture

### 4.1 Technology Stack
- **Framework**: .NET 8.0+ (WPF or WinUI 3 for modern UI)
- **3D Graphics**:
  - SharpDX or Veldrid for 3D rendering
  - Assimp.NET for mesh operations
  - MeshLab integration for advanced processing (optional)
- **Image Processing**:
  - ImageSharp or OpenCV.NET
  - SkiaSharp for 2D vector graphics
- **Geometry Libraries**:
  - Geometry3Sharp for mesh operations
  - Clipper2 for 2D polygon operations
  - Accord.NET for advanced math

### 4.2 File Format Support
- **Import**:
  - Images: PNG, JPG, SVG, BMP, TIFF, GIF
  - 3D Models: STL (for merging/boolean operations)
  - Design files: Proprietary JSON format for saving projects
- **Export**:
  - STL (binary and ASCII)
  - 3MF (with color/material information)
  - OBJ (with textures, optional)
  - G-code (direct generation, optional advanced feature)
  - Project files (.coaster or .json)

### 4.3 Performance Requirements
- **Mesh Generation**:
  - Generate STL in under 5 seconds for standard designs
  - Support meshes up to 500K polygons
  - Real-time preview updates (< 100ms for parameter changes)
- **Memory Management**:
  - Maximum RAM usage: 2GB for typical projects
  - Efficient mesh streaming for large designs
  - Lazy loading for template libraries
- **Responsiveness**:
  - UI remains responsive during mesh generation
  - Background processing with progress indicators
  - Cancellable operations

---

## 5. Advanced Features

### 5.1 Design Library & Sharing
- **Project Management**:
  - Save/load complete projects with all parameters
  - Export project as shareable template
  - Version history tracking
- **Community Integration**:
  - Browse online template gallery
  - Upload designs to community library
  - Rating and comment system
  - Download trending designs

### 5.2 Print Preparation
- **Pre-flight Checks**:
  - Mesh validation (manifold check, normal orientation)
  - Printability analysis (overhangs, thin walls)
  - Recommended supports detection
  - First layer area calculation
- **Material Calculator**:
  - Filament weight and length estimates
  - Cost calculation based on material price
  - Print time estimation
  - Multiple material support for AMS printing

### 5.3 Batch Operations
- **Set Creation**:
  - Design matching sets (4, 6, or 8 coasters)
  - Theme variations (color, pattern)
  - Auto-arrange on print bed
  - Optimized spacing for sequential printing
- **Personalization**:
  - Variable text (names, initials, numbers)
  - CSV import for batch personalization
  - Preview grid for all variants

### 5.4 Special Features
- **Lithophane Coasters**:
  - Image-to-lithophane conversion
  - Thickness optimization for backlighting
  - Preview with simulated light source
  - Recommended white/translucent filament settings
- **Functional Elements**:
  - Bottle opener integration
  - Phone stand conversion
  - Pencil holder attachment points
  - Magnet/weight recesses
- **Artistic Effects**:
  - Watercolor edge effects
  - Gradient height transitions
  - Parametric mathematical patterns (fractals, etc.)
  - Photo mosaic from multiple images

---

## 6. Quality Assurance

### 6.1 Testing Requirements
- **Mesh Quality**:
  - Automated manifold checking
  - No inverted normals
  - No self-intersections
  - Minimum wall thickness validation
- **Print Testing**:
  - Test prints on multiple Bambu models
  - Validation of dimensional accuracy (±0.2mm)
  - Surface quality verification
  - Adhesion testing
- **User Acceptance**:
  - Beta testing program with 50+ users
  - Feedback collection and iteration
  - Bug tracking and resolution

### 6.2 Documentation
- **User Documentation**:
  - Quick start guide
  - Comprehensive manual (PDF and web)
  - Video tutorials for key features
  - Troubleshooting guide
- **Technical Documentation**:
  - API documentation for extensibility
  - File format specifications
  - Mesh generation algorithms explained
  - Best practices guide for print optimization

---

## 7. Future Enhancements (Post-MVP)

- **AI-Powered Features**:
  - Image enhancement and upscaling
  - Smart background removal
  - Auto-design suggestions based on image content
  - Style transfer from reference designs
- **Plugin System**:
  - Third-party pattern generators
  - Custom export formats
  - Integration with other CAD tools
- **Web/Cloud Features**:
  - Web-based version with WebGL
  - Cloud rendering for complex meshes
  - Cross-platform sync (Windows, Mac, Linux)
- **Advanced Materials**:
  - Resin printing support
  - Multi-material optimization beyond AMS
  - Support for exotic materials (carbon fiber, metal-fill)
- **Marketplace**:
  - Premium template store
  - Designer commission system
  - Commercial licensing options

---

## 8. Success Metrics

- **Performance**: < 5 second STL generation for 90% of designs
- **Quality**: > 95% successful first prints without modifications
- **Usability**: New users can create first coaster in < 10 minutes
- **Adoption**: 1000+ active users in first 6 months
- **Community**: 500+ shared designs in first year
- **Satisfaction**: 4.5+ star average rating from users

---

## 9. Development Phases

### Phase 1 - MVP (3-4 months)
- Basic shapes (circle, square, hexagon)
- Image import and height map generation
- Standard STL export
- Simple UI with essential parameters
- Basic Bambu printer profiles

### Phase 2 - Enhanced Features (2-3 months)
- All shapes and edge styles
- Text embossing/debossing
- Multi-layer designs
- Template library
- Advanced Bambu integration (AMS support)

### Phase 3 - Advanced Capabilities (2-3 months)
- Lithophane mode
- Batch operations
- Community features
- Advanced 3D patterns
- Print preparation tools

### Phase 4 - Polish & Scale (ongoing)
- Performance optimization
- Extended documentation
- Community building
- Plugin architecture
- Cross-platform support

---

## 10. Technical Constraints & Considerations

- **File Size**: Keep STL files under 50MB for smooth Bambu Studio import
- **Mesh Complexity**: Balance detail vs. slicing time (target < 5 min slice time)
- **Print Bed Limitations**: Respect maximum build volumes of Bambu printers
- **Material Compatibility**: Ensure designs work with common materials
- **Safety**: No sharp edges that could cause injury in final printed product
- **Licensing**: Ensure all included libraries are commercially compatible

---

*This requirements document should be treated as a living document and updated based on user feedback, technical discoveries, and market needs throughout development.*
