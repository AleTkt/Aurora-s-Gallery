$(document).ready(function () {
    $("#loginForm").on("submit", function (event) {
        event.preventDefault();

        $.ajax({
            url: "/Usuario/IniciarSesion",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (resp) {
                if (resp.success) {
                    window.location.href = "/Home/Index";
                } else {
                    toastr.error(resp.message); //Error tiene cuatro opciones, Success, Warning, Error e Info
                }
            },
            error: function (jqxhr, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor");
                console.error('Error AJAX: ', textStatus, errorThrown);
                console.error('Respuesta del servidor: ', jqxhr.responseText);
            },
        });
    });

    $("#registerForm").on("submit", function (event) {
        event.preventDefault();

        //Valida que el checkbox que hay en el HTML esté marcado 
        if (!$("#terms").is(":checked")) {
            toastr.warning("Debes aceptar los Términos y Condiciones para registrarte.");
            return;
        }

        //Se hace la validación de si ambas contraseñas son iguales o no
        var contra = $("#contrasenha").val();
        var confirmarContra = $("#confirmar_contrasenha").val();

        if (contra !== confirmarContra) {
            toastr.warning("Las contraseñas no coinciden. Por favor, verifícalas.");
            return; // Detiene la ejecución si no son iguales
        }

        //Prepara los datos de los input para pasarlos al controller
        var datosRegistro = {
            usuario: $("#usuario").val(),          
            nombre_completo: $("#nombre_completo").val(),   
            correo: $("#correo").val(),              
            contrasenha: contra                        
        };

        $.ajax({
            url: "/Usuario/Registrarse",
            type: "POST",
            data: datosRegistro,
            success: function (resp) {
                if (resp.success) {
                    toastr.success(resp.message);

                    //Hace una pequeña pausa de dos segundos para que el usuario vea el mensaje de contexto antes de mandarlo al login
                    setTimeout(function () {
                        window.location.href = "/Usuario/Login";
                    }, 2000);

                } else {
                    toastr.error(resp.message);
                }
            },
            error: function (jqxhr, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor al registrar");
                console.error('Error AJAX (Registro): ', textStatus, errorThrown);
                console.error('Detalle: ', jqxhr.responseText);
            }
        });
    });

    $("#frmRecuperar").on("submit", function (event) {
        event.preventDefault();
        $.ajax({
            url: "/Usuario/RecuperarContrasenha",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (resp) {
                if (resp.success) {
                    toastr.success(resp.msg);
                    setTimeout(function () {
                        window.location.href = "/Usuario/Login";
                    }, 3000);
                } else {
                    toastr.error(resp.msg);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor");
                console.error('Error AJAX:', textStatus, errorThrown);
                console.error('Respuesta:', jqXHR.responseText);
            }
        });
    });

    $("#formSolicitud").on("submit", function (event) {
        event.preventDefault();

        $.ajax({
            url: "/Usuario/EnviarSolicitud",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (resp) {

                if (resp.success) {
                    switch (resp.cod) {
                        case 0:
                            // Advertencias (Ej. Correo ya registrado o solicitud duplicada)
                            toastr.warning(resp.msg);
                            break;

                        case 1:
                            // Éxito
                            toastr.success(resp.msg);
                            $('#formSolicitud')[0].reset();

                            // Redirigir al Login después de 3 segundos
                            setTimeout(function () {
                                window.location.href = "/Usuario/Login";
                            }, 3000);
                            break;

                        case 99:
                            // Error del Catch en SQL
                            toastr.error(resp.msg);
                            break;
                    }
                } else {
                    // Errores del ModelState
                    toastr.error(resp.msg);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor.");
                console.error('Error AJAX: ', textStatus, errorThrown);
                console.error('Respuesta: ', jqXHR.responseText);
            }
        });
    });

    $("#formAceptarSolicitud").on("submit", function (event) {
        event.preventDefault();

        // Puedes agregar un loading button aquí si quieres, ya que el envío de correos toma 1 o 2 segundos.
        var btnSubmit = $(this).find('button[type="submit"]');
        var textoOriginal = btnSubmit.html();
        btnSubmit.html('<i class="fa-solid fa-spinner fa-spin me-2"></i> Procesando...').prop('disabled', true);

        $.ajax({
            url: "/Usuario/AceptarSolicitud",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (resp) {
                btnSubmit.html(textoOriginal).prop('disabled', false); // Restauramos el botón

                if (resp.success) {
                    $('#aceptarSolicitudModal').modal('hide');
                    toastr.success(resp.msg);

                    // Recargamos la tabla para que la solicitud desaparezca
                    setTimeout(function () {
                        location.reload();
                    }, 2500);
                } else {
                    toastr.error(resp.msg);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                btnSubmit.html(textoOriginal).prop('disabled', false);
                toastr.error("Error inesperado en el servidor al intentar aprobar.");
                console.error('Error AJAX:', textStatus, errorThrown);
            }
        });
    }); 

    $("#frmAgregarUsuario").on("submit", function (event) {
        event.preventDefault(); 

        $.ajax({
            url: "/Usuario/AgregarUsuario",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (resp) {
                if (resp.success) {

                    switch (resp.cod) {

                        case 0:
                            toastr.warning(resp.msg);

                            break;
                        case 1:
                            $('#agregarUsuarioModal').modal('hide');
                            toastr.success(resp.msg);
                            $('#frmAgregarUsuario')[0].reset();
                            setTimeout(function () {
                                location.reload();
                            }, 3000);
                            break;
                        case 99:
                            toastr.error(resp.msg);
                            break;
                    }

                } else {
                    toastr.error(resp.msg);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor.");
                console.error('Error AJAX: ', textStatus, errorThrown);
                console.error('Respuesta: ', jqXHR,responseText)
            }
        });
    });

    $("#frmModificarUsuario").on("submit", function (event) {
        event.preventDefault();

        $.ajax({
            url: "/Usuario/ModificarUsuario",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (resp) {
                if (resp.success) {

                    switch (resp.cod) {

                        case 0:
                            toastr.warning(resp.msg);

                            break;
                        case 1:
                            $('#modificarUsuarioModal').modal('hide');
                            toastr.success(resp.msg);
                            $('#frmModificarUsuario')[0].reset();
                            setTimeout(function () {
                                location.reload();
                            }, 3000);
                            break;
                        case 99:
                            toastr.error(resp.msg);
                            break;
                    }

                } else {
                    toastr.error(resp.msg);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor.");
                console.error('Error AJAX: ', textStatus, errorThrown);
                console.error('Respuesta: ', jqXHR, responseText)
            }
        });
    });

    $("#a_usuario, #usuario_m").on("blur", function () {
        var $input = $(this);
        $input.removeClass("is-invalid is-valid");

        if ($input.val() === "") {
            return;
        }

        $.ajax({
            url: " /Usuario/ValidarUsuario",
            type: "POST",
            data: { usuario: $input.val().trim(), id: $("#id_usuario_m").val() },
            success: function (resp) {
                if (resp.success) {
                    $input.addClass("is-invalid");
                    $input.focus();

                }
                else {
                    $input.addClass("is-valid");
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor.");
                console.error('Error AJAX: ', textStatus, errorThrown);
                console.error('Respuesta: ', jqXHR, responseText)
            }
        });
    });

    $("#correo, #correo_m").on("blur", function () {
        var $input = $(this);
        $input.removeClass("is-invalid is-valid");

        if ($input.val() === "") {
            return;
        }

        $.ajax({
            url: " /Usuario/ValidarCorreo",
            type: "POST",
            data: { correo: $input.val().trim(), id: $("#id_usuario_m").val() },
            success: function (resp) {
                if (resp.success) {
                    $input.addClass("is-invalid");
                    $input.focus();

                }
                else {
                    $input.addClass("is-valid");
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error("Error inesperado en el servidor.");
                console.error('Error AJAX: ', textStatus, errorThrown);
                console.error('Respuesta: ', jqXHR, responseText)
            }
        });
    });
}); // Cierre

function cargarDatosUsuario(id) {
    $.ajax({
        url: "/Usuario/BuscarUsuario",
        type: "POST",
        data: { id: id }, 
        success: function (resp) {
            $("#id_usuario_m").val(resp.id);
            $("#usuario_m").val(resp.usuario);
            $("#nombre_corto_m").val(resp.nombre_corto);
            $("#nombre_completo_m").val(resp.nombre_completo);
            $("#correo_m").val(resp.correo);
            $("#rol_m").val(resp.id_rol);
            $("#estado_m").val(resp.id_estado);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            toastr.error("Error inesperado en el servidor");
            console.error("Error AJAX:", textStatus, errorThrown);
            console.error("Respuesta:", jqXHR.responseText);
        }
    });
}