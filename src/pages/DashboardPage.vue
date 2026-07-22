<template>
  <q-page class="dashboard-page">
      <q-table
        :rows="rows"
        :columns="columns"
        :filter="filter"
        row-key="uid"
        :rows-per-page-options="[100, 200, 500]"
        :pagination="{ rowsPerPage: 100 }"
        class="dashboard-table"
        dense
        :loading="loading"
        flat
        bordered
      >
        <template v-slot:top-left>
          <div class="row q-gutter-md items-center">
            <q-btn
              :style="{ backgroundColor: '#f44336', color: 'white' }"
              push
              icon="add"
              label="Nuevo certificado"
              dense
              @click="abrirDialog"
            />
            <q-input
              v-model="filtroFechaDesde"
              label="Fecha de expedición Desde"
              type="date"
              outlined
              dense
              stack-label
              class="q-ml-md"
            />
            <q-input
              v-model="filtroFechaHasta"
              label="Fecha de expedición Hasta"
              type="date"
              outlined
              dense
              stack-label
            />
            <q-btn
              :style="{ backgroundColor: '#f44336', color: 'white' }"
              icon="search"
              label="Buscar"
              dense
              push
              :disable="rolId === 2 && (!filtroFechaDesde || !filtroFechaHasta)"
              @click="manejarBusqueda"
            />
          </div>
        </template>

        <template v-slot:top-right>
          <div class="row q-gutter-md items-center">
            <q-input
              v-model="filter"
              outlined
              dense
              debounce="300"
              placeholder="Buscar..."
              style="min-width: 250px"
            >
              <template v-slot:append>
                <q-icon name="search" />
              </template>
            </q-input>
            <q-btn
              :style="{ backgroundColor: '#f44336', color: 'white' }"
              icon="file_download"
              dense
              push
              round
              @click="exportarCSV"
            >
              <q-tooltip>Exportar CSV</q-tooltip>
            </q-btn>
          </div>
        </template>

        <template v-slot:body-cell-titular="props">
          <q-td :props="props">
            <div class="row items-center no-wrap">
              <q-btn
                v-if="esAdmin && !props.row.procesado"
                flat
                dense
                round
                icon="delete"
                size="sm"
                color="red-6"
                class="q-mr-xs"
                @click.stop="confirmarBorrarCertificado(props.row)"
              >
                <q-tooltip>Borrar certificado</q-tooltip>
              </q-btn>
              <q-icon
                v-if="props.row.procesado"
                name="lock"
                size="14px"
                color="grey-7"
                class="q-mr-xs"
              />
            <a
              href="#"
              class="titular-link"
              @click.prevent="editarCertificado(props.row)"
            >
              {{ props.value }}
            </a>
            </div>
          </q-td>
        </template>

        <template v-slot:body-cell-numeroContrato="props">
          <q-td :props="props">
            <a
              v-if="esAdmin"
              href="#"
              class="titular-link"
              @click.prevent="abrirEdicionContrato(props.row)"
            >
              {{ props.value || '' }}
            </a>
            <span v-else>{{ props.value || '' }}</span>
          </q-td>
        </template>

        <template v-slot:body-cell-agencia="props">
          <q-td :props="props">
            {{ props.value || '' }}
          </q-td>
        </template>

        <template v-slot:body-cell-fechaExpedicion="props">
          <q-td :props="props">
            {{ formatDate(props.value) }}
          </q-td>
        </template>

        <template v-slot:body-cell-vigenteDesde="props">
          <q-td :props="props">
            {{ formatDate(props.value) }}
          </q-td>
        </template>

        <template v-slot:body-cell-vigenteHasta="props">
          <q-td :props="props">
            {{ formatDate(props.value, 'DD/MM/YYYY') }}
          </q-td>
        </template>

        <template v-slot:body-cell-tipoVehiculo="props">
          <q-td :props="props">
            {{ props.row.tipoVehiculo === 'NU' ? 'NUEVO' : props.row.tipoVehiculo === 'SE' ? 'SEMINUEVO' : props.value || '' }}
          </q-td>
        </template>

        <template v-slot:body-cell-acciones="props">
          <q-td :props="props">
            <q-btn
              flat
              dense
              round
              icon="picture_as_pdf"
              color="red"
              @click="descargarCertificado(props.row)"
            >
              <q-tooltip>Descargar certificado</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>

    <!-- Dialog para nuevo certificado o modificación -->
    <q-dialog v-model="dialogOpen" persistent>
      <q-card class="certificado-dialog column no-wrap relative-position" style="min-width: 650px; max-width: 650px;">

        <div class="row no-wrap col certificado-dialog__body">
          <div class="col column no-wrap certificado-dialog__main">

            <q-bar class="certificado-dialog__header">
              <q-icon name="assignment" size="sm" class="q-mr-sm" />
              <div class="text-subtitle1 text-weight-bold">
                {{ modoEdicion ? 'Modificación de certificado' : 'Nuevo certificado' }}
              </div>
              <q-space />
              <q-btn dense flat icon="close" @click="cerrarDialog" />
            </q-bar>

            <q-card-section class="col scroll q-pa-md">
              <q-form ref="formRef" @submit="guardarCertificado" id="formCertificado"  class="q-gutter-md">
                <!-- Nombre, apellidos y Número de contrato (tercios iguales) -->
                <div class="row q-gutter-md">
                  <div class="col">
                    <q-input
                      ref="nombreTitularRef"
                      v-model="formulario.nombreTitular"
                      label="Nombre"
                      outlined
                      dense
                      :rules="[val => !!val || 'El nombre es requerido']"
                      lazy-rules
                      stack-label
                      autofocus
                    />
                  </div>
                  <div class="col">
                    <q-input
                      ref="apellidosTitularRef"
                      v-model="formulario.apellidosTitular"
                      label="Apellidos"
                      outlined
                      dense
                      :rules="[val => !!val || 'Los apellidos son requeridos']"
                      lazy-rules
                      stack-label
                    />
                  </div>
                  <div class="col">
                    <q-input
                      ref="numeroContratoRef"
                      v-model="formulario.numeroContrato"
                      label="Número de contrato"
                      outlined
                      dense
                      stack-label
                      mask="##########"
                      maxlength="10"
                      :loading="verificandoContrato"
                      :rules="[
                        val => !!val || 'El número de contrato es requerido',
                        val => /^\d{10}$/.test(String(val || '').trim()) || 'Debe tener exactamente 10 dígitos numéricos',
                        val => !errorContratoDuplicado || 'Este número de contrato ya existe'
                      ]"
                      lazy-rules
                      hint="Escriba el número de contrato a 10 dígitos"
                      @update:model-value="verificarContratoDuplicado"
                    />
                  </div>
                </div>
                <!-- Selección coberturas-->
              <div class="row q-gutter-md">
                <div class="col">
                <q-table
                    :rows="coberturasAgregadas"
                    :columns="coberturaColumns"
                    row-key="cobertura"
                    dense
                    flat
                    bordered
                    hide-pagination
                    class=" full-width ticket-cobertura-qtable"
                    :rows-per-page-options="[0]"
            
                  >
                    <template v-slot:header-cell-acciones="props">
                      <q-th :props="props">
                        <q-btn-dropdown
                          ref="coberturaDropdownRef"
                          label="AGREGAR"
                          class="ticket-btn--add-cobertura"
                          dense
                          size="sm"
                          no-caps
                          unelevated
                          dropdown-icon="expand_more"
                          content-class="ticket-cobertura-dropdown"
                          :loading="coberturasCatalogoLoading"
                          @before-show="cargarCoberturas"
                          table-class="ticket-cobertura-qtable__zebra"
                        >
                          <q-list class="ticket-cobertura-dropdown__list">
                            <q-item-label header class="ticket-cobertura-dropdown__header">
                              <q-icon name="verified_user" class="q-mr-sm" />
                              Selecciona una cobertura
                            </q-item-label>

                            <q-item
                              v-for="(item, index) in coberturasCatalogoDisponibles"
                              :key="item.value ?? `cobertura-${index}`"
                              dense
                              clickable
                              v-close-popup
                              class="ticket-cobertura-dropdown__item"
                              @click="agregarCobertura(item)"
                            >
                              <q-item-section>
                                <q-item-label class="ticket-cobertura-dropdown__label">{{ item.label }}</q-item-label>
                                <q-item-label caption class="ticket-cobertura-dropdown__caption">Cobertura #{{ item.value }}</q-item-label>
                              </q-item-section>

                              <q-item-section side>
                                <q-icon name="add_circle" class="ticket-cobertura-dropdown__add-icon" />
                              </q-item-section>
                            </q-item>

                            <q-item
                              v-if="!coberturasCatalogoLoading && coberturasCatalogoDisponibles.length === 0"
                              dense
                              class="ticket-cobertura-dropdown__empty"
                              disable
                            >
                              <q-item-section avatar>
                                <q-avatar class="ticket-cobertura-dropdown__avatar ticket-cobertura-dropdown__avatar--empty" size="38px">
                                  <q-icon name="info" size="20px" />
                                </q-avatar>
                              </q-item-section>
                              <q-item-section>
                                <q-item-label class="text-grey-7">
                                  {{
                                    opcionesCoberturas.length === 0
                                      ? 'Sin coberturas disponibles'
                                      : 'Todas las coberturas agregadas'
                                  }}
                                </q-item-label>
                              </q-item-section>
                            </q-item>
                          </q-list>
                        </q-btn-dropdown>
                      </q-th>
                    </template>

                    <template v-slot:body-cell-acciones="props">
                      <q-td :props="props" class="text-center">
                        <q-btn
                          flat
                          dense
                          round
                          size="sm"
                          icon="delete"
                          color="negative"
                          @click="eliminarCobertura(props.rowIndex)"
                        />
                      </q-td>
                    </template>
                    <template v-slot:body-cell-primaNeta="props">
                      <q-td :props="props">
                        {{ Number(props.row.primaNeta || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 }) }}
                      </q-td>
                    </template>

                    <template v-slot:body-cell-primaTotal="props">
                      <q-td :props="props">
                        {{ Number(props.row.primaTotal || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 }) }}
                      </q-td>
                    </template>
                     <template v-slot:bottom-row>
                      <q-tr v-if="coberturasAgregadas.length > 0" class="ticket-cobertura-qtable__total-row">
                        <q-td class="text-weight-bold">Total Anual</q-td>
                      <q-td class="text-weight-bold text-right">
                        <q-icon name="attach_money" size="16px" class="q-mr-xs" />{{ primaNetaTotal.toLocaleString('es-MX', { minimumFractionDigits: 2 }) }}
                      </q-td>
                      <q-td class="text-weight-bold text-right">
                        <q-icon name="attach_money" size="16px" class="q-mr-xs" />{{ primaTotalTotal.toLocaleString('es-MX', { minimumFractionDigits: 2 }) }}
                      </q-td>
                        <q-td></q-td>
                      </q-tr>
                     <q-tr v-if="coberturasAgregadas.length > 0" class="ticket-cobertura-qtable__total-anios-row">
                        <q-td class="text-weight-bold">Total Vigencia ({{ formulario.aniosVigencia }} {{ formulario.aniosVigencia === 1 ? 'año' : 'años' }})</q-td>
                        <q-td class="text-weight-bold text-right">
                          <q-icon name="attach_money" size="14px" class="q-mr-xs" />{{ primaNetaTotalPorVigencia.toLocaleString('es-MX', { minimumFractionDigits: 2 }) }}
                        </q-td>
                        <q-td class="text-weight-bold text-right">
                          <q-icon name="attach_money" size="14px" class="q-mr-xs" />{{ primaTotalTotalVigencia.toLocaleString('es-MX', { minimumFractionDigits: 2 }) }}
                        </q-td>
                        <q-td></q-td>
                      </q-tr>
                    </template>

                    <template v-slot:no-data>
                      <div class="full-width text-center text-grey">
                        Sin coberturas agregadas
                      </div>
                    </template>
                  </q-table>
                  </div>
                </div>

                <!-- Fecha expedición y Años vigencia -->
                <div class="row q-gutter-md">
                  <div class="col">
                    <q-input
                      ref="fechaExpedicionRef"
                      v-model="formulario.fechaExpedicion"
                      label="Fecha expedición"
                      type="date"
                      outlined
                      dense
                      :rules="[val => !!val || 'La fecha de expedición es requerida']"
                      lazy-rules
                      readonly
                      @update:model-value="calcularFechas"
                    />
                  </div>
                  <div class="col">
                    <q-select
                      ref="aniosVigenciaRef"
                      v-model="formulario.aniosVigencia"
                      :options="opcionesAnios"
                      label="Años vigencia"
                      outlined
                      dense
                      :rules="[val => val !== null && val !== undefined && val !== '' || 'Los años de vigencia son requeridos']"
                      lazy-rules
                      emit-value
                      map-options
                      @update:model-value="calcularFechas"
                    >
                      <template v-slot:option="scope">
                        <q-item
                          v-bind="scope.itemProps"
                          class="bg-blue-grey-2 text-bold"
                        >
                          <q-item-section>
                            <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>

                <!-- Vigente desde y Vigente hasta (calculados) -->
                <div class="row q-gutter-md">
                  <div class="col">
                    <q-input
                      v-model="formulario.vigenteDesde"
                      label="Vigente desde"
                      type="date"
                      outlined
                      dense
                      readonly
                    />
                  </div>
                  <div class="col">
                    <q-input
                      v-model="formulario.vigenteHasta"
                      label="Vigente hasta"
                      type="date"
                      outlined
                      dense
                      readonly
                    />
                  </div>
                </div>

                <!-- Tipo de Vehículo, Marca y Submarca -->
                <div class="row q-gutter-md">
                  <div class="col">
                    <q-select
                      ref="tipoVehiculoRef"
                      v-model="formulario.tipoVehiculo"
                      :options="opcionesTipoVehiculo"
                      label="Tipo de Vehículo"
                      outlined
                      dense
                      :rules="[val => !!val || 'El tipo de vehículo es requerido']"
                      lazy-rules
                      emit-value
                      map-options
                    >
                      <template v-slot:option="scope">
                        <q-item
                          v-bind="scope.itemProps"
                          class="bg-blue-grey-2 text-bold"
                        >
                          <q-item-section>
                            <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                          </q-item-section>
                        </q-item>
                      </template>
                      <template v-slot:after>
                        <q-btn
                          v-if="formulario.tipoVehiculo === 'SE'"
                          round
                          dense
                          flat
                          icon="directions_car"
                          color="primary"
                          @click.stop="abrirBusquedaVehiculo"
                        >
                          <q-tooltip>Buscar vehículo</q-tooltip>
                        </q-btn>
                      </template>
                    </q-select>
                  </div>
                  <div class="col">
                    <!-- Seminuevo: Marca como q-input -->
                    <q-input
                      v-if="formulario.tipoVehiculo === 'SE'"
                      ref="marcaRef"
                      v-model="formulario.marca"
                      label="Marca"
                      outlined
                      dense
                      readonly
                      :rules="[val => !!val || 'La marca es requerida']"
                      lazy-rules
                    />
                    <!-- Nuevo: Marca como q-select -->
                    <q-select
                      v-else-if="formulario.tipoVehiculo === 'NU'"
                      ref="marcaRef"
                      v-model="formulario.marca"
                      :options="opcionesMarcas"
                      label="Marca"
                      outlined
                      dense
                      :rules="[val => !!val || 'La marca es requerida']"
                      lazy-rules
                      emit-value
                      map-options
                    >
                      <template v-slot:option="scope">
                        <q-item
                          v-bind="scope.itemProps"
                          class="bg-blue-grey-2 text-bold"
                        >
                          <q-item-section>
                            <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                  <div class="col">
                    <!-- Seminuevo: Submarca / Versión como q-input -->
                    <q-input
                      v-if="formulario.tipoVehiculo === 'SE'"
                      ref="submarcaRef"
                      v-model="formulario.submarca"
                      label="Submarca / Versión"
                      outlined
                      dense
                      readonly
                      :rules="[val => !!val || 'La submarca es requerida']"
                      lazy-rules
                    />
                    <!-- Nuevo: Submarca como q-select -->
                    <q-select
                      v-else-if="formulario.tipoVehiculo === 'NU'"
                      ref="submarcaRef"
                      v-model="formulario.submarca"
                      :options="opcionesSubmarcasFiltradas"
                      label="Submarca / Versión"
                      outlined
                      dense
                      :rules="[val => !!val || 'La submarca es requerida']"
                      lazy-rules
                      use-input
                      input-debounce="0"
                      @filter="filtrarSubmarcas"
                      @new-value="crearNuevaSubmarca"
                      new-value-mode="add-unique"
                      fill-input
                      hide-selected
                      emit-value
                      map-options
                      :disable="!formulario.marca"
                    >
                      <template v-slot:option="scope">
                        <q-item
                          v-bind="scope.itemProps"
                          class="bg-blue-grey-2 text-bold"
                        >
                          <q-item-section>
                            <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                          </q-item-section>
                        </q-item>
                      </template>
                      <template v-slot:no-option>
                        <q-item>
                          <q-item-section class="text-grey">
                            Escriba para buscar o presione Enter para agregar
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>

                <!-- Versión (ancho completo, solo Seminuevo) -->
                <div v-if="formulario.tipoVehiculo === 'SE'" class="row q-gutter-md">
                  <div class="col-12">
                    <q-input
                      v-model="formulario.version"
                      label="Versión"
                      outlined
                      dense
                      readonly
                    />
                  </div>
                </div>

                <!-- Modelo y No. de serie -->
                <div class="row q-gutter-md">
                  <div class="col">
                    <q-input
                      ref="modeloRef"
                      v-model="formulario.modelo"
                      label="Modelo"
                      outlined
                      dense
                      :readonly="formulario.tipoVehiculo !== 'NU'"
                      :mask="formulario.tipoVehiculo === 'NU' ? '####' : undefined"
                      inputmode="numeric"
                      :rules="reglasModelo"
                      lazy-rules
                    />
                  </div>
                  <div class="col">
                    <q-input
                      ref="numeroSerieRef"
                      v-model="formulario.numeroSerie"
                      label="No. de serie"
                      outlined
                      dense
                      :rules="[val => !!val || 'El número de serie es requerido']"
                      lazy-rules
                    />
                  </div>
                </div>
              </q-form>
            </q-card-section>
             <!-- Botones de acción -->
              <q-card-actions align="right" class=" certificado-dialog__actions q-pt-md">
                <q-btn
                  flat
                  label="Cancelar"
                  color="negative"
                  dense
                  push
                  @click="cerrarDialog"
                />
                <q-btn
                  type="submit"
                  form="formCertificado"
                  :label="modoEdicion ? 'Actualizar' : 'Guardar'"
                  :style="{ backgroundColor: '#f44336', color: 'white' }"
                  dense
                  push
                  :loading="guardando"
                  :disable="modoEdicion && certificadoProcesado"
                />
              </q-card-actions>
          </div>
        </div>

      </q-card>
    </q-dialog>
    <!--                    --> 

    <!-- Dialog para editar número de contrato (solo admin) -->
    <q-dialog v-model="dialogEditarContratoOpen">
      <q-card style="min-width: 260px; max-width: 45vw;">
        <q-card-section>
          <div class="row items-center justify-between">
            <div class="text-h6">Editar número de contrato</div>
            <q-btn flat round dense icon="close" @click="dialogEditarContratoOpen = false" />
          </div>
        </q-card-section>
        <q-card-section class="q-pt-none">
          <q-input
            v-model="contratoEditNumero"
            label="Número de contrato"
            outlined
            dense
            autofocus
          />
        </q-card-section>
        <q-card-actions align="right">
          <q-btn
            flat
            label="Cerrar"
            color="negative"
            dense
            @click="dialogEditarContratoOpen = false"
          />
          <q-btn
            push
            label="Guardar"
            dense
            :style="{ backgroundColor: '#ff8000', color: 'white' }"
            :loading="guardandoContrato"
            :disable="!contratoEditNumero || contratoEditNumero.trim() === ''"
            @click="guardarEdicionContrato"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Dialog Búsqueda de vehículo (cuando tipo es Nuevo) -->
    <q-dialog v-model="dialogBusquedaVehiculoOpen" @keyup.escape="dialogBusquedaVehiculoOpen = false">
      <q-card style="min-width: 720px; max-width: 90vw;">
        <q-card-section>
          <div class="row items-center justify-between">
            <div class="text-h6">Búsqueda de vehículo</div>
            <q-btn
              flat
              round
              dense
              icon="close"
              @click="dialogBusquedaVehiculoOpen = false"
            />
          </div>
        </q-card-section>
        <q-card-section class="q-pt-none">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-4">
              <q-input
                v-model="busquedaAnio"
                label="Año"
                outlined
                dense
                clearable
                stack-label
                placeholder="Ej. 2024"
                :rules="reglasBusquedaAnio"
                mask="####"
                maxlength="4"
                autofocus
                @keyup.enter="puedeBuscarVehiculo && ejecutarBusquedaVehiculo()"
              />
            </div>
            <div class="col-12 col-sm-4">
              <q-input
                v-model="busquedaMarca"
                label="Marca"
                outlined
                dense
                clearable
                stack-label
                placeholder="Ej. BYD"
                :rules="[val => !!val || 'La marca es obligatoria']"
                @keyup.enter="puedeBuscarVehiculo && ejecutarBusquedaVehiculo()"
              />
            </div>
            <div class="col-12 col-sm-4">
              <q-input
                v-model="busquedaSubtipo"
                label="Subtipo (opcional)"
                outlined
                dense
                clearable
                stack-label
                placeholder="Ej. SUV"
                @keyup.enter="puedeBuscarVehiculo && ejecutarBusquedaVehiculo()"
              />
            </div>
          </div>
          <div class="row q-mt-md">
            <div class="col-12">
              <q-btn
                label="Buscar"
                :style="{ backgroundColor: '#f44336', color: 'white' }"
                class="full-width"
                no-caps
                push
                :disable="!puedeBuscarVehiculo"
                :loading="busquedaVehiculoLoading"
                @click="ejecutarBusquedaVehiculo"
              />
            </div>
          </div>
          <div class="row q-mt-lg">
            <div class="col-12">
              <q-table
                :rows="resultadosBusquedaVehiculo"
                :columns="columnasBusquedaVehiculo"
                :filter="filterBusquedaVehiculo"
                row-key="_rowKey"
                selection="single"
                v-model:selected="selectedVehiculoBusqueda"
                flat
                bordered
                dense
                :loading="busquedaVehiculoLoading"
                :rows-per-page-options="[5, 10]"
                class="tabla-busqueda-vehiculo"
              >
                <template v-slot:top-right>
                  <q-input
                    v-model="filterBusquedaVehiculo"
                    outlined
                    dense
                    debounce="300"
                    placeholder="Buscar..."
                    clearable
                    style="min-width: 200px"
                  >
                    <template v-slot:append>
                      <q-icon name="search" />
                    </template>
                  </q-input>
                </template>
              </q-table>
            </div>
          </div>
          <div class="row q-mt-md">
            <div class="col-12">
              <q-btn
                label="Aceptar"
                color="primary"
                class="full-width"
                no-caps
                :disable="selectedVehiculoBusqueda.length === 0"
                @click="aceptarVehiculoSeleccionado"
              />
            </div>
          </div>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Dialog para selección de agencia -->
    <q-dialog v-model="dialogAgenciaOpen" persistent>
      <q-card style="min-width: 600px; max-width: 800px;">
        <q-banner dense class="bg-black q-pa-md">
          <div class="row items-center justify-between full-width">
            <div class="text-h6 text-primary">Seleccionar Agencia</div>
            <q-btn
              v-if="mostrarBotonCerrar"
              flat
              dense
              round
              size="sm"
              icon="close"
              color="white"
              text-color="white"
              @click="dialogAgenciaOpen = false"
            />
          </div>
        </q-banner>

        <q-card-section>
          <div class="text-subtitle2 text-grey-7">
            Por favor, selecciona una agencia para continuar
          </div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-input
            v-model="filtroAgencia"
            outlined
            dense
            debounce="300"
            placeholder="Buscar agencia..."
            class="q-mb-md"
            autofocus
          >
            <template v-slot:prepend>
              <q-icon name="search" />
            </template>
            <template v-slot:append v-if="filtroAgencia">
              <q-icon
                name="clear"
                class="cursor-pointer"
                @click="filtroAgencia = ''"
              />
            </template>
          </q-input>
        </q-card-section>

        <q-card-section class="q-pt-none" style="max-height: 60vh; overflow-y: auto;">
          <q-list separator>
            <q-item
              v-for="(agencia, index) in agenciasFiltradas"
              :key="index"
              clickable
              v-ripple
              @click="seleccionarAgencia(agencia)"
              class="q-pa-sm q-mb-sm"
              style="border: 1px solid #e0e0e0; border-radius: 8px; margin-bottom: 8px;"
            >
              <q-item-section>
                <q-item-label class="text-weight-medium text-body2">
                  {{ agencia.agencia || 'N/A' }}
                </q-item-label>
                <q-item-label caption class="q-mt-xs text-caption">
                  <div class="column q-gutter-xs">
                    <div v-if="agencia.bbvaAgenciaId" class="row items-center">
                      <q-icon name="tag" size="12px" class="q-mr-xs" />
                      <span class="text-weight-medium">Agencia ID:</span>
                      <span class="q-ml-xs">{{ agencia.bbvaAgenciaId }}</span>
                    </div>
                    <div v-if="agencia.entidadFederativa" class="row items-center">
                      <q-icon name="location_on" size="12px" class="q-mr-xs" />
                      <span class="text-weight-medium">Estado:</span>
                      <span class="q-ml-xs">{{ agencia.entidadFederativa }}</span>
                    </div>
                    <div v-if="agencia.division" class="row items-center">
                      <q-icon name="business" size="12px" class="q-mr-xs" />
                      <span class="text-weight-medium">División:</span>
                      <span class="q-ml-xs">{{ agencia.division }}</span>
                    </div>
                  </div>
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-icon name="chevron_right" color="primary" size="20px" />
              </q-item-section>
            </q-item>
          </q-list>
          <div v-if="agenciasFiltradas.length === 0" class="text-center text-grey-6 q-pa-md">
            No se encontraron agencias
          </div>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Dialog para filtros de búsqueda (RolId == 1) -->
    <q-dialog v-model="dialogFiltrosBusqueda" persistent>
      <q-card style="min-width: 500px; max-width: 600px;">
        <q-banner dense class="bg-black q-pa-md">
          <div class="row items-center justify-between full-width">
            <div class="text-h6 text-primary">Filtros de Búsqueda</div>
            <q-btn
              flat
              dense
              round
              size="sm"
              icon="close"
              color="white"
              text-color="white"
              @click="dialogFiltrosBusqueda = false"
            />
          </div>
        </q-banner>

        <q-card-section class="q-pt-md">
          <div class="column q-gutter-md">
            <div>
              <q-select
                v-model="filtrosSeleccionados.perfiles"
                :options="opcionesPerfiles"
                label="Perfiles"
                outlined
                dense
                multiple
                use-chips
                use-input
                input-debounce="0"
                @filter="filtrarPerfiles"
                emit-value
                map-options
              >
                <template v-slot:no-option>
                  <q-item>
                    <q-item-section class="text-grey">
                      No hay resultados
                    </q-item-section>
                  </q-item>
                </template>
              </q-select>
              <div class="q-mt-xs row q-gutter-sm">
                <a
                  v-if="!todoPerfilesSeleccionado"
                  href="#"
                  class="text-primary text-small cursor-pointer"
                  style="font-size: 0.75rem; text-decoration: none;"
                  @click.prevent="seleccionarTodosPerfiles"
                >
                  Seleccionar todo
                </a>
                <a
                  v-if="filtrosSeleccionados.perfiles && filtrosSeleccionados.perfiles.length >= 2"
                  href="#"
                  class="text-primary text-small cursor-pointer"
                  style="font-size: 0.75rem; text-decoration: none;"
                  @click.prevent="limpiarPerfiles"
                >
                  Limpiar todo
                </a>
              </div>
            </div>

            <div>
              <q-select
                v-model="filtrosSeleccionados.divisiones"
                :options="opcionesDivisiones"
                label="Divisiones"
                outlined
                dense
                multiple
                use-chips
                use-input
                input-debounce="0"
                @filter="filtrarDivisiones"
                emit-value
                map-options
              >
                <template v-slot:no-option>
                  <q-item>
                    <q-item-section class="text-grey">
                      No hay resultados
                    </q-item-section>
                  </q-item>
                </template>
              </q-select>
              <div class="q-mt-xs row q-gutter-sm">
                <a
                  v-if="!todoDivisionesSeleccionado"
                  href="#"
                  class="text-primary text-small cursor-pointer"
                  style="font-size: 0.75rem; text-decoration: none;"
                  @click.prevent="seleccionarTodasDivisiones"
                >
                  Seleccionar todo
                </a>
                <a
                  v-if="filtrosSeleccionados.divisiones && filtrosSeleccionados.divisiones.length >= 2"
                  href="#"
                  class="text-primary text-small cursor-pointer"
                  style="font-size: 0.75rem; text-decoration: none;"
                  @click.prevent="limpiarDivisiones"
                >
                  Limpiar todo
                </a>
              </div>
            </div>
          </div>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat label="Cancelar" color="grey" @click="dialogFiltrosBusqueda = false" />
          <q-btn 
            push 
            label="Aplicar Filtros" 
            color="primary" 
            @click="aplicarFiltrosBusqueda"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from 'src/stores/auth'
import { authService } from 'src/services/authService'
import { certificadoService } from 'src/services/certificadoService'
import { catalogoVehiculosService } from 'src/services/catalogoVehiculosService'
import { catalogoCoberturasService } from 'src/services/catalogoCoberturasService'
import { useQuasar, date } from 'quasar'

const router = useRouter()
const authStore = useAuthStore()
const $q = useQuasar()

const loading = ref(false)
const filter = ref('')
const dialogOpen = ref(false)
const guardando = ref(false)
const modoEdicion = ref(false)
const cargandoEdicion = ref(false)
const certificadoEditando = ref(null) // Guardar el uid del certificado que se está editando
const certificadoProcesado = ref(false)
// Edición de contrato (admin)
const dialogEditarContratoOpen = ref(false)
const contratoEditUid = ref(null)
const contratoEditNumero = ref('')
const guardandoContrato = ref(false)
const dialogAgenciaOpen = ref(false)
const filtroAgencia = ref('')
const mostrarBotonCerrar = ref(false)
const verificandoContrato = ref(false)
const errorContratoDuplicado = ref(false)
let timeoutVerificacion = null

// Filtros de fecha para búsqueda
const filtroFechaDesde = ref('')
const filtroFechaHasta = ref('')

// Dialog y filtros para búsqueda avanzada (RolId == 1)
const dialogFiltrosBusqueda = ref(false)
const filtrosSeleccionados = ref({
  perfiles: [],
  divisiones: []
})

// Obtener RolId del store
const rolId = computed(() => {
  return authStore.rolId || authService.getRolId()
})

const esAdmin = computed(() => rolId.value === 1)

async function confirmarBorrarCertificado(row) {
  if (!esAdmin.value) return
  const uid = row?.uid
  if (!uid) return

  $q.dialog({
    title: 'Confirmación',
    html: true,
    message: `
      <div>¿Confirmas borrar el certificado?</div>
      <table style="margin-top: 12px; width: 100%; border-collapse: collapse; border: 1px solid #eeeeee; background: #fafafa;">
        <tr style="background: #ffffff;">
          <td style="padding: 4px 8px; width: 45%; color: rgba(0,0,0,0.7);">No Certificado</td>
          <td style="padding: 4px 8px;"><b>${row?.noCertificado || ''}</b></td>
        </tr>
        <tr style="background: #f5f5f5;">
          <td style="padding: 4px 8px; color: rgba(0,0,0,0.7);">No Contrato</td>
          <td style="padding: 4px 8px;"><b>${row?.numeroContrato || ''}</b></td>
        </tr>
        <tr style="background: #ffffff;">
          <td style="padding: 4px 8px; color: rgba(0,0,0,0.7);">Titular</td>
          <td style="padding: 4px 8px;"><b>${row?.titular || ''}</b></td>
        </tr>
      </table>
    `,
    persistent: true,
    cancel: {
      label: 'Cancelar',
      flat: true,
      color: 'negative'
    },
    ok: {
      label: 'Confirmar borrar',
      push: true,
      color: 'negative'
    }
  }).onOk(async () => {
    try {
      await certificadoService.borrarCertificado({ uid })
      const idx = rows.value.findIndex(r => r.uid === uid)
      if (idx !== -1) rows.value.splice(idx, 1)
      $q.notify({ type: 'positive', message: 'Certificado borrado correctamente' })
      if (modoEdicion.value && certificadoEditando.value === uid) {
        cerrarDialog()
      }
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data || err.message || 'Error al borrar certificado.'
      $q.notify({ type: 'negative', message: typeof msg === 'string' ? msg : 'Error al borrar certificado.' })
    }
  })
}

// Obtener Rol del usuario
const rol = computed(() => {
  const userInfo = authStore.user || authService.getUserInfo()
  return userInfo?.rol || userInfo?.Rol || null
})

// Obtener la división del usuario desde userInfo
const divisionUsuario = computed(() => {
  const userInfo = authStore.user || authService.getUserInfo()
  return userInfo?.division || userInfo?.Division || null
})

// Obtener perfiles y divisiones del store
const perfiles = computed(() => {
  return authStore.perfiles || authService.getPerfiles() || []
})

const divisiones = computed(() => {
  return authStore.divisiones || authService.getDivisiones() || []
})

// Preparar opciones para los q-select de perfiles
const opcionesPerfilesCompletas = computed(() => {
  if (!perfiles.value || !Array.isArray(perfiles.value)) {
    return []
  }
  // Si son objetos, mapearlos a {label, value}, si son strings, convertirlos
  return perfiles.value.map((perfil, index) => {
    if (typeof perfil === 'object' && perfil !== null) {
      return {
        label: perfil.label || perfil.nombre || perfil.perfil || String(perfil),
        value: perfil.value || perfil.id || perfil.perfilId || index
      }
    }
    return {
      label: String(perfil),
      value: perfil
    }
  })
})

// Preparar opciones para los q-select de divisiones
const opcionesDivisionesCompletas = computed(() => {
  // Si el rol es "STAFF DIVISIONAL", solo mostrar la división del usuario
  if (rol.value === 'STAFF DIVISIONAL' && divisionUsuario.value) {
    return [{
      label: String(divisionUsuario.value),
      value: divisionUsuario.value
    }]
  }
  
  // Si no hay divisiones o no es un array, retornar vacío
  if (!divisiones.value || !Array.isArray(divisiones.value)) {
    return []
  }
  
  // Si son objetos, mapearlos a {label, value}, si son strings, convertirlos
  return divisiones.value.map((division, index) => {
    if (typeof division === 'object' && division !== null) {
      return {
        label: division.label || division.nombre || division.division || String(division),
        value: division.value || division.id || division.divisionId || index
      }
    }
    return {
      label: String(division),
      value: division
    }
  })
})

// Opciones filtradas para perfiles (para el filtro del q-select)
const opcionesPerfiles = ref([])
const opcionesDivisiones = ref([])

// Inicializar opciones
watch([opcionesPerfilesCompletas, opcionesDivisionesCompletas], () => {
  opcionesPerfiles.value = opcionesPerfilesCompletas.value
  opcionesDivisiones.value = opcionesDivisionesCompletas.value
}, { immediate: true })

// Función para filtrar perfiles en el q-select
const filtrarPerfiles = (val, update) => {
  if (val === '') {
    update(() => {
      opcionesPerfiles.value = opcionesPerfilesCompletas.value
    })
    return
  }
  update(() => {
    const needle = val.toLowerCase()
    opcionesPerfiles.value = opcionesPerfilesCompletas.value.filter(
      v => v.label.toLowerCase().indexOf(needle) > -1
    )
  })
}

// Función para filtrar divisiones en el q-select
const filtrarDivisiones = (val, update) => {
  if (val === '') {
    update(() => {
      opcionesDivisiones.value = opcionesDivisionesCompletas.value
    })
    return
  }
  update(() => {
    const needle = val.toLowerCase()
    opcionesDivisiones.value = opcionesDivisionesCompletas.value.filter(
      v => v.label.toLowerCase().indexOf(needle) > -1
    )
  })
}

// Función para manejar el click en el botón Buscar
const manejarBusqueda = () => {
  // Si es RolId == 1 y Rol == 'GR', ejecutar búsqueda normal (como RolId == 2)
  if (rolId.value === 1 && rol.value === 'GR') {
    buscarCertificados()
  } else if (rolId.value === 1) {
    // Si es RolId == 1 pero Rol != 'GR', abrir el dialog de filtros
    // Si el rol es "STAFF DIVISIONAL", seleccionar automáticamente la división del usuario
    if (rol.value === 'STAFF DIVISIONAL' && divisionUsuario.value) {
      filtrosSeleccionados.value.divisiones = [divisionUsuario.value]
    }
    dialogFiltrosBusqueda.value = true
  } else {
    // Si es RolId == 2, ejecutar la búsqueda normal
    buscarCertificados()
  }
}

// Función para aplicar los filtros de búsqueda
const aplicarFiltrosBusqueda = async () => {
  // Validar que al menos uno de los q-select tenga elementos seleccionados
  const perfilesSeleccionados = filtrosSeleccionados.value.perfiles || []
  const divisionesSeleccionadas = filtrosSeleccionados.value.divisiones || []
  
  if (perfilesSeleccionados.length === 0 && divisionesSeleccionadas.length === 0) {
    $q.notify({
      type: 'negative',
      message: 'Debe seleccionar al menos un perfil o una división para aplicar los filtros',
      position: 'top',
      timeout: 4000
    })
    return
  }
  
  loading.value = true
  
  try {
    // Convertir los valores seleccionados a strings (labels)
    const perfilesStrings = perfilesSeleccionados.map(valor => {
      const opcion = opcionesPerfilesCompletas.value.find(opt => opt.value === valor)
      return opcion ? opcion.label : String(valor)
    })
    
    const divisionesStrings = divisionesSeleccionadas.map(valor => {
      const opcion = opcionesDivisionesCompletas.value.find(opt => opt.value === valor)
      return opcion ? opcion.label : String(valor)
    })
    
    // Crear el payload con arreglos de strings y fechas
    const payload = {
      perfiles: perfilesStrings,
      divisiones: divisionesStrings,
      fechaExpedicionDesde: filtroFechaDesde.value || '',
      fechaExpedicionHasta: filtroFechaHasta.value || ''
    }
    
    // Imprimir el payload en la consola
    console.log('Payload de filtros:', payload)
    
    // Hacer el POST a la API
    const response = await certificadoService.consultarCertificados(payload)
    
    // Imprimir la respuesta en la consola
    console.log('Respuesta de la API:', response)
    
    // Procesar el response de la misma manera que cargarCertificados
    // Verificar si fue exitoso (code === 0)
    if (response.code === 0 && response.certificados) {
      rows.value = response.certificados
      // Verificar si el array está vacío
      if (response.certificados.length === 0) {
        $q.notify({
          type: 'info',
          message: 'No se encontraron registros con los filtros seleccionados',
          position: 'top',
          timeout: 3000
        })
      }
    } else if (response.code === 0 && Array.isArray(response.data)) {
      // Si la respuesta viene en data en lugar de certificados
      rows.value = response.data
      // Verificar si el array está vacío
      if (response.data.length === 0) {
        $q.notify({
          type: 'info',
          message: 'No se encontraron registros con los filtros seleccionados',
          position: 'top',
          timeout: 3000
        })
      }
    } else {
      // Si no hay certificados, inicializar array vacío
      rows.value = []
      
      // Mostrar mensaje de que no se encontraron registros
      const mensaje = response.message || 'No se encontraron registros con los filtros seleccionados'
      $q.notify({
        type: 'info',
        message: mensaje,
        position: 'top',
        timeout: 3000
      })
    }
    
    // Cerrar el dialog
    dialogFiltrosBusqueda.value = false
    
    $q.notify({
      type: 'positive',
      message: 'Filtros aplicados correctamente',
      position: 'top',
      timeout: 2000
    })
  } catch (error) {
    console.error('Error al aplicar filtros:', error)
    
    const errorMessage = error.response?.data?.mensaje || 
                        error.response?.data?.message || 
                        'Error al aplicar los filtros'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 3000
    })
    
    // En caso de error, inicializar array vacío
    rows.value = []
  } finally {
    loading.value = false
  }
}

// Computed para verificar si todos los perfiles están seleccionados
const todoPerfilesSeleccionado = computed(() => {
  if (!opcionesPerfilesCompletas.value || opcionesPerfilesCompletas.value.length === 0) {
    return false
  }
  if (!filtrosSeleccionados.value.perfiles || filtrosSeleccionados.value.perfiles.length === 0) {
    return false
  }
  return filtrosSeleccionados.value.perfiles.length === opcionesPerfilesCompletas.value.length
})

// Computed para verificar si todas las divisiones están seleccionadas
const todoDivisionesSeleccionado = computed(() => {
  if (!opcionesDivisionesCompletas.value || opcionesDivisionesCompletas.value.length === 0) {
    return false
  }
  if (!filtrosSeleccionados.value.divisiones || filtrosSeleccionados.value.divisiones.length === 0) {
    return false
  }
  return filtrosSeleccionados.value.divisiones.length === opcionesDivisionesCompletas.value.length
})

// Función para limpiar la selección de perfiles
const limpiarPerfiles = () => {
  filtrosSeleccionados.value.perfiles = []
}

// Función para limpiar la selección de divisiones
const limpiarDivisiones = () => {
  filtrosSeleccionados.value.divisiones = []
}

// Función para seleccionar todos los perfiles
const seleccionarTodosPerfiles = () => {
  filtrosSeleccionados.value.perfiles = opcionesPerfilesCompletas.value.map(opt => opt.value)
}

// Función para seleccionar todas las divisiones
const seleccionarTodasDivisiones = () => {
  filtrosSeleccionados.value.divisiones = opcionesDivisionesCompletas.value.map(opt => opt.value)
}

// Refs para los campos del formulario
const formRef = ref(null)
const nombreTitularRef = ref(null)
const apellidosTitularRef = ref(null)
const numeroContratoRef = ref(null)
const fechaExpedicionRef = ref(null)
const aniosVigenciaRef = ref(null)
const tipoVehiculoRef = ref(null)
const marcaRef = ref(null)
const submarcaRef = ref(null)
const modeloRef = ref(null)
const numeroSerieRef = ref(null)

const opcionesCoberturas = ref([])
const coberturasAgregadas = ref([])
const coberturasCatalogoLoading = ref(false)
const coberturaDropdownRef = ref(null)

const coberturasCatalogoDisponibles = computed(() => {
  const agregados = new Set(coberturasAgregadas.value.map((c) => c.cobertura))
  return opcionesCoberturas.value.filter((item) => !agregados.has(item.value))
})

// Formulario para nuevo certificado
const formulario = ref({
  nombreTitular: '',
  apellidosTitular: '',
  numeroContrato: '',
  cobertura: null,
  fechaExpedicion: '',
  aniosVigencia: null,
  vigenteDesde: '',
  vigenteHasta: '',
  tipoVehiculo: '',
  marca: '',
  submarca: '',
  modelo: '',
  numeroSerie: '',
  descripcionVehiculo: '',
  version: '',
  usuario: '',
  creadoPor: '',
  estado: 'Solicitado',
  Ampara: '',
  primaNeta: 0,
  primaTotal: 0
})

function construirTitularDesdeForm(f) {
  const n = (f?.nombreTitular || '').trim()
  const a = (f?.apellidosTitular || '').trim()
  return [n, a].filter(Boolean).join(' ')
}

function descomponerTitularParaForm(row) {
  const titularRaw = (row?.titular || '').trim()
  const nom = row?.nombre ?? row?.Nombre
  const ape = row?.apellidos ?? row?.Apellidos

  const nombreCampo = nom != null ? String(nom).trim() : ''
  const apellidosCampo = ape != null ? String(ape).trim() : ''

  // Si viene nombre/apellidos por campos, usarlos... pero si "titular" trae más apellidos,
  // preferir lo que se pueda inferir del titular (ej. "MANUEL GARCIA F" vs apellidos "GARCIA").
  if (nombreCampo) {
    if (titularRaw && titularRaw.toLowerCase().startsWith(nombreCampo.toLowerCase() + ' ')) {
      const apellidosDesdeTitular = titularRaw.slice(nombreCampo.length).trim()
      if (apellidosDesdeTitular && apellidosDesdeTitular.length > apellidosCampo.length) {
        return { nombre: nombreCampo, apellidos: apellidosDesdeTitular }
      }
    }
    return { nombre: nombreCampo, apellidos: apellidosCampo }
  }

  if (!titularRaw) return { nombre: '', apellidos: '' }
  const i = titularRaw.indexOf(' ')
  if (i === -1) return { nombre: titularRaw, apellidos: '' }
  return { nombre: titularRaw.slice(0, i).trim(), apellidos: titularRaw.slice(i + 1).trim() }
}

// Opciones para años de vigencia
const opcionesAnios = [
  { label: '1', value: 1 },
  { label: '2', value: 2 },
  { label: '3', value: 3 },
  { label: '4', value: 4 },
  { label: '5', value: 5 },
  { label: '6', value: 6 }
]

// Opciones para tipo de vehículo
const opcionesTipoVehiculo = [
  { label: 'Nuevo', value: 'NU' }
]

// Opciones para marca
const opcionesMarcas = [
  { label: 'BYD', value: 'BYD' }
]

// Estructura de submarcas por marca
const submarcasPorMarca = {
  byd: [
    { modelo: 'Dolphin Mini', anio: 2026 },
    { modelo: 'Yuan Pro EV', anio: 2026 },
    { modelo: 'Seal EV', anio: 2026 },
    { modelo: 'Sealion 7 EV', anio: 2026 },
    { modelo: 'Atto 8', anio: 2026 },
    { modelo: 'King DM-i', anio: 2026 },
    { modelo: 'Song Pro DM-i', anio: 2026 },
    { modelo: 'Song Plus DM-i', anio: 2026 },
    { modelo: 'Yuan Pro DM-i', anio: 2026 },
    { modelo: 'Shark DMO', anio: 2026 },
    { modelo: 'M9', anio: 2026 }
  ]
}

// Opciones de submarcas filtradas (se actualiza según la marca seleccionada y el filtro de búsqueda)
const opcionesSubmarcasFiltradas = ref([])

// Computed para obtener las submarcas según la marca seleccionada (ordenadas alfabéticamente)
const opcionesSubmarcas = computed(() => {
  if (!formulario.value.marca) {
    return []
  }
  
  const marcaKey = formulario.value.marca.toLowerCase()
  const submarcas = submarcasPorMarca[marcaKey] || []
  
  // Ordenar alfabéticamente por modelo
  const submarcasOrdenadas = [...submarcas].sort((a, b) => {
    return a.modelo.localeCompare(b.modelo, 'es', { sensitivity: 'base' })
  })
  
  // Convertir a formato para q-select: { label, value }
  return submarcasOrdenadas.map(item => ({
    label: item.modelo,
    value: item.modelo
  }))
})

// Función para filtrar submarcas mientras el usuario escribe
const filtrarSubmarcas = (val, update) => {
  if (val === '') {
    update(() => {
      opcionesSubmarcasFiltradas.value = opcionesSubmarcas.value
    })
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    opcionesSubmarcasFiltradas.value = opcionesSubmarcas.value.filter(
      v => v.label.toLowerCase().indexOf(needle) > -1
    )
  })
}

// Función para crear una nueva submarca cuando el usuario escribe algo que no está en la lista
const crearNuevaSubmarca = (val, done) => {
  if (val && val.length > 0) {
    // Agregar el nuevo valor como opción temporal para que se pueda seleccionar
    const nuevaOpcion = { label: val, value: val }
    // Verificar que no exista ya en las opciones
    const existe = opcionesSubmarcasFiltradas.value.some(opt => opt.value === val)
    if (!existe) {
      opcionesSubmarcasFiltradas.value.push(nuevaOpcion)
    }
    // Aceptar el valor y asignarlo directamente al formulario
    done(val, 'add-unique')
  }
}

// Watcher para asignar el año del modelo al seleccionar una submarca
watch(() => formulario.value.submarca, (submarca) => {
  if (cargandoEdicion.value || !submarca || formulario.value.tipoVehiculo !== 'NU') return

  const marcaKey = (formulario.value.marca || '').toLowerCase()
  const item = (submarcasPorMarca[marcaKey] || []).find(s => s.modelo === submarca)
  if (item?.anio) {
    formulario.value.modelo = String(item.anio)
  }
})

// Watcher para limpiar submarca cuando cambie la marca (no limpiar si estamos cargando datos de edición)
watch(() => formulario.value.marca, (nuevaMarca, marcaAnterior) => {
  if (cargandoEdicion.value) return
  if (marcaAnterior !== undefined && marcaAnterior !== nuevaMarca) {
    formulario.value.submarca = ''
  }
})

// Año calendario actual a 4 dígitos (modelo automático cuando tipo es Nuevo)
const anioActualModelo = () => String(new Date().getFullYear())

// Reglas del campo Modelo: en Nuevo, año a 4 dígitos = año siguiente, actual o anterior; en Seminuevo, obligatorio (valor desde búsqueda)
const reglasModelo = computed(() => {
  const tipo = formulario.value.tipoVehiculo
  if (tipo === 'NU') {
    const y = new Date().getFullYear()
    const ySig = y + 1
    const yAnt = y - 1
    return [
      val => !!val || 'El modelo es requerido',
      val => /^\d{4}$/.test(String(val || '').trim()) || 'Debe ser un número de 4 dígitos',
      val => {
        const n = parseInt(String(val || '').trim(), 10)
        if (isNaN(n)) return 'Debe ser un año válido'
        return (n === ySig || n === y || n === yAnt) || `Solo se permite el año ${ySig}, ${y} o ${yAnt}`
      }
    ]
  }
  return [val => !!val || 'El modelo es requerido']
})

// Watcher para limpiar campos y abrir búsqueda de vehículo cuando cambie el tipo de vehículo (no limpiar si estamos cargando datos de edición)
watch(() => formulario.value.tipoVehiculo, (nuevoValor) => {
  if (cargandoEdicion.value) return
  formulario.value.marca = nuevoValor === 'NU' ? 'BYD' : ''
  formulario.value.submarca = ''
  formulario.value.modelo = nuevoValor === 'NU' ? anioActualModelo() : ''
  formulario.value.descripcionVehiculo = ''
  formulario.value.version = ''
  // Mostrar diálogo de Búsqueda de vehículo solo cuando se selecciona Seminuevo
  if (nuevoValor === 'SE') {
    abrirBusquedaVehiculo()
  }
})

// --- Búsqueda de vehículo (diálogo cuando tipo es Seminuevo) ---
const dialogBusquedaVehiculoOpen = ref(false)
const busquedaAnio = ref('')
const busquedaMarca = ref('')
const busquedaSubtipo = ref('')
const resultadosBusquedaVehiculo = ref([])
const selectedVehiculoBusqueda = ref([])
const busquedaVehiculoLoading = ref(false)
const filterBusquedaVehiculo = ref('')

// Reglas de validación para Año: obligatorio, 4 dígitos numéricos, entre 2018 y 2026
const reglasBusquedaAnio = [
  val => !!val || 'El año es obligatorio',
  val => /^\d{4}$/.test(String(val || '').trim()) || 'Debe ser un valor numérico de 4 dígitos',
  val => {
    const n = parseInt(String(val || '').trim(), 10)
    if (isNaN(n)) return true
    return (n >= 2018 && n <= 2026) || 'El año debe estar entre 2018 y 2026'
  }
]

// Habilita el botón Buscar solo si Año y Marca están capturados y el año es válido (2018-2026)
const puedeBuscarVehiculo = computed(() => {
  const anio = String(busquedaAnio.value || '').trim()
  const marca = String(busquedaMarca.value || '').trim()
  if (!anio || !marca) return false
  if (!/^\d{4}$/.test(anio)) return false
  const n = parseInt(anio, 10)
  return n >= 2018 && n <= 2026
})

const columnasBusquedaVehiculo = [
  { name: 'segmento', label: 'Segmento', field: 'segmento', align: 'left', sortable: true },
  { name: 'marca', label: 'Marca', field: 'marca', align: 'left', sortable: true },
  { name: 'subMarca', label: 'Sub tipo', field: 'subMarca', align: 'left', sortable: true },
  { name: 'modelo', label: 'Año', field: 'modelo', align: 'left', sortable: true },
  { name: 'descripcion', label: 'Descripción', field: 'descripcion', align: 'left', sortable: false },
  { name: 'transmision', label: 'Transmisión', field: 'transmision', align: 'left', sortable: true }
]

// Listado plano de vehículos para búsqueda (segmento, marca, subTipo, modelo, descripcion, transmision, anio)
const listadoVehiculosCompleto = computed(() => {
  const lista = []
  const marcas = { byd: 'BYD' }
  for (const [key, items] of Object.entries(submarcasPorMarca)) {
    const marcaLabel = marcas[key] || key
    for (const item of items) {
      lista.push({
        segmento: '',
        marca: marcaLabel,
        subTipo: item.modelo,
        modelo: item.modelo,
        descripcion: item.modelo,
        transmision: 'Automática',
        anio: String(item.anio)
      })
    }
  }
  return lista.sort((a, b) => a.modelo.localeCompare(b.modelo, 'es', { sensitivity: 'base' }))
})

async function ejecutarBusquedaVehiculo() {
  const modelo = parseInt(String(busquedaAnio.value || '').trim(), 10)
  const marca = String(busquedaMarca.value || '').trim()
  const subtipo = String(busquedaSubtipo.value || '').trim() || null
  if (isNaN(modelo) || modelo < 2018 || modelo > 2026 || !marca) return
  busquedaVehiculoLoading.value = true
  selectedVehiculoBusqueda.value = []
  try {
    const data = await catalogoVehiculosService.buscarVehiculos(modelo, marca, subtipo || undefined)
    if (data.code === 0 && Array.isArray(data.vehiculos)) {
      // Mapear response: subMarca/subTipo → Sub tipo, descripción → Descripción, transmisión → Transmisión, modelo → Año
      resultadosBusquedaVehiculo.value = data.vehiculos.map((v, i) => {
        const subMarcaVal = v.subMarca ?? v.SubMarca ?? v.subTipo ?? v.SubTipo ?? ''
        return {
          _rowKey: `vehiculo-${i}-${v.marca}-${subMarcaVal}-${(v.descripción ?? v.descripcion ?? '').slice(0, 30)}`,
          segmento: v.segmento ?? '',
          marca: v.marca ?? '',
          subMarca: subMarcaVal,
          subTipo: subMarcaVal,
          modelo: v.modelo ?? '',
          descripcion: v.descripción ?? v.descripcion ?? '',
          transmision: v.transmisión ?? v.transmision ?? ''
        }
      })
      if (data.vehiculos.length === 0) {
        $q.notify({ type: 'info', message: 'No se encontraron vehículos con los criterios indicados.' })
      }
    } else {
      resultadosBusquedaVehiculo.value = []
      $q.notify({ type: 'warning', message: data.message || 'No se obtuvieron resultados.' })
    }
  } catch (err) {
    resultadosBusquedaVehiculo.value = []
    const msg = err.response?.data?.message || err.response?.data || err.message || 'Error al buscar vehículos.'
    $q.notify({ type: 'negative', message: typeof msg === 'string' ? msg : 'Error al buscar vehículos.' })
  } finally {
    busquedaVehiculoLoading.value = false
  }
}

function abrirBusquedaVehiculo() {
  busquedaAnio.value = ''
  busquedaMarca.value = ''
  busquedaSubtipo.value = ''
  filterBusquedaVehiculo.value = ''
  resultadosBusquedaVehiculo.value = []
  selectedVehiculoBusqueda.value = []
  dialogBusquedaVehiculoOpen.value = true
}

function aceptarVehiculoSeleccionado() {
  const selected = selectedVehiculoBusqueda.value[0]
  if (!selected) return
  const row = resultadosBusquedaVehiculo.value.find(r => r._rowKey === selected._rowKey) || selected
  const submarcaVal = (row.subMarca ?? '').toString()
  formulario.value.modelo = String(row.modelo ?? '')
  formulario.value.marca = (row.marca ?? '').toString()
  formulario.value.descripcionVehiculo = (row.descripcion ?? '').toString()
  formulario.value.version = (row.descripcion ?? '').toString()
  dialogBusquedaVehiculoOpen.value = false
  nextTick(() => {
    formulario.value.submarca = submarcaVal
  })
}

function abrirEdicionContrato(row) {
  if (!esAdmin.value) return
  contratoEditUid.value = row.uid
  contratoEditNumero.value = row.numeroContrato || ''
  dialogEditarContratoOpen.value = true
}

async function guardarEdicionContrato() {
  if (!esAdmin.value) return
  const uid = contratoEditUid.value
  const numeroContrato = (contratoEditNumero.value || '').trim()
  if (!uid || !numeroContrato) return

  $q.dialog({
    title: 'Confirmación',
    message: '¿Deseas guardar el nuevo número de contrato?',
    persistent: true,
    cancel: {
      label: 'Cancelar',
      flat: true
    },
    ok: {
      label: 'Guardar',
      push: true,
      style: { backgroundColor: '#ff8000', color: 'white' }
    }
  }).onOk(async () => {
    guardandoContrato.value = true
    try {
      await certificadoService.editarContratoCertificado({
        Uid: uid,
        NumeroContrato: numeroContrato
      })

      const idx = rows.value.findIndex(r => r.uid === uid)
      if (idx !== -1) {
        rows.value[idx] = {
          ...rows.value[idx],
          numeroContrato
        }
      }

      if (modoEdicion.value && certificadoEditando.value === uid) {
        formulario.value.numeroContrato = numeroContrato
      }

      $q.notify({ type: 'positive', message: 'Contrato actualizado correctamente' })
      dialogEditarContratoOpen.value = false
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data || err.message || 'Error al editar contrato.'
      $q.notify({ type: 'negative', message: typeof msg === 'string' ? msg : 'Error al editar contrato.' })
    } finally {
      guardandoContrato.value = false
    }
  })
}

const rows = ref([])

// Definición de columnas (basadas en el response del API)
const columns = [
  {
    name: 'uid',
    required: true,
    label: 'ID',
    align: 'left',
    field: row => row.uid,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'noCertificado',
    required: true,
    label: 'Número de Certificado',
    align: 'left',
    field: row => row.noCertificado,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'titular',
    required: true,
    label: 'Titular',
    align: 'left',
    field: row => row.titular,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'numeroContrato',
    label: 'Número de contrato',
    align: 'left',
    field: row => row.numeroContrato,
    format: val => val || '',
    sortable: true
  },
  {
    name: 'agencia',
    label: 'Agencia',
    align: 'left',
    field: row => row.agencia,
    format: val => val || '',
    sortable: true
  },
  {
    name: 'fechaExpedicion',
    label: 'Fecha Expedición',
    align: 'left',
    field: row => row.fechaExpedicion,
    sortable: true
  },
  {
    name: 'vigenteDesde',
    label: 'Inicio Vigencia',
    align: 'left',
    field: row => row.vigenteDesde,
    sortable: true
  },
  {
    name: 'vigenteHasta',
    label: 'Fin Vigencia',
    align: 'left',
    field: row => row.vigenteHasta,
    sortable: true
  },
  {
    name: 'aniosVigencia',
    label: 'Años Vigencia',
    align: 'left',
    field: row => row.aniosVigencia,
    sortable: true
  },
  {
    name: 'tipoVehiculo',
    label: 'Tipo Vehículo',
    align: 'left',
    field: row => row.tipoVehiculo,
    format: val => val === 'NU' ? 'NUEVO' : val === 'SE' ? 'SEMINUEVO' : val || '',
    sortable: true
  },
  {
    name: 'marca',
    label: 'Marca',
    align: 'left',
    field: row => row.marca,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'submarca',
    label: 'Submarca',
    align: 'left',
    field: row => row.submarca,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'modelo',
    label: 'Modelo',
    align: 'left',
    field: row => row.modelo,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'serie',
    label: 'Serie',
    align: 'left',
    field: row => row.serie,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'Ampara',
    label: 'Ampara',
    align: 'left',
    field: row => row.ampara,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'PrimaNeta',
    label: 'Prima Neta',
    align: 'left',
    field: row => row.primaNeta,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'PrimaTotal',
    label: 'Prima Total',
    align: 'left',
    field: row => row.primaTotal,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'status',
    label: 'Estatus',
    align: 'left',
    field: row => row.polizaStatus,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'creadoPor',
    label: 'Creado Por',
    align: 'left',
    field: row => row.creadoPor,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'acciones',
    label: 'Acciones',
    align: 'center',
    field: 'acciones',
    sortable: false
  }
]

const coberturaColumns = [
  { name: 'coberturaLabel', label: 'Cobertura', field: 'coberturaLabel', align: 'left' },
  { name: 'primaNeta', label: 'Prima Neta', field: 'primaNeta', align: 'right' },
  { name: 'primaTotal', label: 'Prima Total', field: 'primaTotal', align: 'right' },
  { name: 'acciones', label: '', field: 'acciones', align: 'center' }
]

// Función para obtener el color del chip según polizaStatusId
const getPolizaStatusColor = (polizaStatusId) => {
  if (polizaStatusId === 7) {
    return {
      color: 'deep-orange-5',
      textColor: 'white'
    }
  } else if (polizaStatusId === 4) {
    return {
      color: 'red',
      textColor: 'white'
    }
  } else if (polizaStatusId === 3) {
    return {
      color: 'amber-11',
      textColor: 'amber-10'
    }
  } else if (polizaStatusId === 2) {
    return {
      color: 'green',
      textColor: 'white'
    }
  }
  // Color por defecto si no coincide con ninguno
  return {
    color: 'grey',
    textColor: 'white'
  }
}

// Función para exportar los datos a CSV
const exportarCSV = () => {
  if (rows.value.length === 0) {
    $q.notify({
      type: 'warning',
      message: 'No hay datos para exportar',
      position: 'top',
      timeout: 3000
    })
    return
  }

  try {
    // Obtener los nombres de las columnas (excluyendo 'acciones')
    const columnasExportar = columns.filter(col => col.name !== 'acciones')
    const headers = columnasExportar.map(col => col.label).join(',')

    // Convertir cada row a una línea CSV
    const csvRows = rows.value.map(row => {
      return columnasExportar.map(col => {
        let value = col.field ? col.field(row) : row[col.name]
        
        // Formatear fechas si es necesario
        if (col.name === 'fechaExpedicion' || col.name === 'vigenteDesde' || col.name === 'vigenteHasta') {
          value = formatDate(value)
        }
        
        // Si el valor contiene comas, comillas o saltos de línea, envolverlo en comillas
        if (value != null && (String(value).includes(',') || String(value).includes('"') || String(value).includes('\n'))) {
          value = `"${String(value).replace(/"/g, '""')}"`
        }
        
        return value || ''
      }).join(',')
    })

    // Combinar headers y rows
    const csvContent = [headers, ...csvRows].join('\n')

    // Crear blob y descargar
    const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    
    // Nombre del archivo con fecha actual
    const fechaActual = date.formatDate(new Date(), 'YYYY-MM-DD_HH-mm-ss')
    link.download = `certificados_${fechaActual}.csv`
    
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    $q.notify({
      type: 'positive',
      message: `Se exportaron ${rows.value.length} certificados a CSV`,
      position: 'top',
      timeout: 3000
    })
  } catch (error) {
    console.error('Error al exportar CSV:', error)
    $q.notify({
      type: 'negative',
      message: 'Error al exportar el archivo CSV',
      position: 'top',
      timeout: 5000
    })
  }
}

// Función para formatear fechas usando Quasar date
const formatDate = (dateString, format = 'DD/MM/YYYY') => {
  if (!dateString) return ''
  // Si viene en formato ISO (con T), Quasar date lo maneja automáticamente
  return date.formatDate(dateString, format)
}

// Función para calcular fechas de vigencia
const calcularFechas = () => {
  if (!formulario.value.fechaExpedicion || !formulario.value.aniosVigencia) {
    formulario.value.vigenteDesde = ''
    formulario.value.vigenteHasta = ''
    return
  }

  // Vigente desde es igual a la fecha de expedición
  formulario.value.vigenteDesde = formulario.value.fechaExpedicion

  // Vigente hasta es fecha de expedición + años de vigencia
  const fechaExpedicion = new Date(formulario.value.fechaExpedicion)
  const fechaHasta = new Date(fechaExpedicion)
  fechaHasta.setFullYear(fechaHasta.getFullYear() + formulario.value.aniosVigencia)
  
  // Formatear a YYYY-MM-DD para el input type="date"
  formulario.value.vigenteHasta = fechaHasta.toISOString().split('T')[0]
}

// Función para abrir el diálogo (modo nuevo)
const abrirDialog = () => {
  modoEdicion.value = false
  certificadoEditando.value = null
  certificadoProcesado.value = false
  resetearFormulario(true) // Prellenar con fecha de hoy
  calcularFechas()
  dialogOpen.value = true
}

async function cargarCoberturas() {
  if (opcionesCoberturas.value.length > 0) return

  coberturasCatalogoLoading.value = true
  try {
    const data = await catalogoCoberturasService.obtenerCoberturas()

    opcionesCoberturas.value = data.map(item => ({
      label: item.nombre,
      value: item.uid,
      primaNeta: item.primaNeta,
      primaTotal: item.primaTotal
    }))
  } catch (error) {
    console.error(error)
    opcionesCoberturas.value = []
    $q.notify({
      color: 'red',
      message: error?.message || 'Error al cargar el catálogo de coberturas',
      position: 'top',
    })
  } finally {
    coberturasCatalogoLoading.value = false
  }
}
function agregarCobertura(item) {
  const yaExiste = coberturasAgregadas.value.some(c => c.cobertura === item.value)
  if (yaExiste) {
    Notify.create({ type: 'warning', message: 'Esa cobertura ya fue agregada' })
    return
  }

  coberturasAgregadas.value.push({
    cobertura: item.value,
    coberturaLabel: item.label,
    primaNeta: item.primaNeta,
    primaTotal: item.primaTotal
  })
  coberturaDropdownRef.value?.hide?.()
}

function eliminarCobertura(index) {
  coberturasAgregadas.value.splice(index, 1)
}
const primaNetaTotal = computed(() => {
  return coberturasAgregadas.value.reduce((sum, item) => sum + Number(item.primaNeta || 0), 0)
})
const primaTotalTotal = computed(() => {
  return coberturasAgregadas.value.reduce((sum, item) => sum + Number(item.primaTotal || 0), 0)
})
const primaNetaTotalPorVigencia = computed(() => {
  const anios = Number(formulario.value.aniosVigencia) || 1
  return primaNetaTotal.value * anios
})
const primaTotalTotalVigencia = computed(() => {
  const anios = Number(formulario.value.aniosVigencia) || 1
  return primaTotalTotal.value * anios
})
watch(
  [primaNetaTotalPorVigencia, primaTotalTotalVigencia, coberturasAgregadas],
  ([netaAnios, totalAnios]) => {
    formulario.value.Ampara = coberturasAgregadas.value.map(c => c.coberturaLabel).join(', ')
    formulario.value.primaNeta = netaAnios
    formulario.value.primaTotal = totalAnios
  },
  { deep: true }
)

// Función para editar un certificado (abrir diálogo en modo edición)
const editarCertificado = async (row) => {
  modoEdicion.value = true
  cargandoEdicion.value = true
  certificadoEditando.value = row.uid
  certificadoProcesado.value = !!row.procesado
  
  // Limpiar estados de validación de contrato (no se valida en modo edición)
  errorContratoDuplicado.value = false
  verificandoContrato.value = false
  if (timeoutVerificacion) {
    clearTimeout(timeoutVerificacion)
    timeoutVerificacion = null
  }
  
  // Poblar el formulario con los datos del certificado
  const { nombre: nombreEd, apellidos: apellidosEd } = descomponerTitularParaForm(row)
  formulario.value = {
    nombreTitular: nombreEd,
    apellidosTitular: apellidosEd,
    numeroContrato: row.numeroContrato || '',
    fechaExpedicion: row.fechaExpedicion ? formatDateForInput(row.fechaExpedicion) : '',
    aniosVigencia: calcularAniosVigencia(row.vigenteDesde, row.vigenteHasta),
    vigenteDesde: row.vigenteDesde ? formatDateForInput(row.vigenteDesde) : '',
    vigenteHasta: row.vigenteHasta ? formatDateForInput(row.vigenteHasta) : '',
    tipoVehiculo: row.tipoVehiculo || '',
    marca: row.marca || '',
    submarca: row.submarca || '',
    modelo: row.modelo || '',
    numeroSerie: row.serie || row.numeroSerie || '',
    descripcionVehiculo: row.descripcionVehiculo || '',
    version: row.version || row.Version || '',
    usuario: row.usuario || '',
    creadoPor: row.creadoPor || '',
    estado: row.estado || 'Solicitado',
    primaNeta: row.primaNeta ?? null,
    primaTotal: row.primaTotal ?? null,
    ampara: row.ampara || ''
  }

  coberturasAgregadas.value = (row.coberturas || []).map(c => ({
  cobertura: c.uid,
  coberturaLabel: c.nombre,
  primaNeta: c.primaNeta,
  primaTotal: c.primaTotal
}))

  nextTick(() => {
    cargandoEdicion.value = false
  })

  dialogOpen.value = true
}

// Función para formatear fecha para input type="date" (YYYY-MM-DD)
const formatDateForInput = (dateString) => {
  if (!dateString) return ''
  // Si viene en formato ISO (con T), extraer solo la fecha
  if (dateString.includes('T')) {
    return dateString.split('T')[0]
  }
  // Si viene en formato DD/MM/YYYY, convertir a YYYY-MM-DD
  if (dateString.includes('/')) {
    const parts = dateString.split('/')
    if (parts.length === 3) {
      return `${parts[2]}-${parts[1]}-${parts[0]}`
    }
  }
  // Si ya está en formato YYYY-MM-DD, retornar tal cual
  return dateString
}

// Función para calcular años de vigencia basado en las fechas
const calcularAniosVigencia = (vigenteDesde, vigenteHasta) => {
  if (!vigenteDesde || !vigenteHasta) return null
  
  const desde = new Date(vigenteDesde)
  const hasta = new Date(vigenteHasta)
  
  const diffTime = Math.abs(hasta - desde)
  const diffYears = Math.ceil(diffTime / (1000 * 60 * 60 * 24 * 365))
  
  return diffYears < 1 ? 1 : diffYears
}

// Función para verificar si el contrato está duplicado
const verificarContratoDuplicado = (valor) => {
  // Limpiar timeout anterior si existe
  if (timeoutVerificacion) {
    clearTimeout(timeoutVerificacion)
  }

  // No verificar si está vacío o en modo edición
  if (!valor || valor.trim() === '' || modoEdicion.value) {
    errorContratoDuplicado.value = false
    verificandoContrato.value = false
    return
  }

  // Limpiar error previo
  errorContratoDuplicado.value = false
  
  // Activar loading
  verificandoContrato.value = true

  // Debounce: esperar 500ms antes de hacer la verificación
  timeoutVerificacion = setTimeout(async () => {
    try {
      const response = await certificadoService.verificarContrato(valor.trim())
      
      if (response.contratoExiste === true) {
        errorContratoDuplicado.value = true
        // Forzar validación del campo
        await numeroContratoRef.value?.validate()
      } else {
        errorContratoDuplicado.value = false
      }
    } catch (error) {
      console.error('Error al verificar contrato:', error)
      // En caso de error, no bloquear el formulario
      errorContratoDuplicado.value = false
    } finally {
      verificandoContrato.value = false
    }
  }, 500)
}

// Función para cerrar el diálogo
const cerrarDialog = () => {
  // Limpiar timeout si existe
  if (timeoutVerificacion) {
    clearTimeout(timeoutVerificacion)
    timeoutVerificacion = null
  }
  
  dialogOpen.value = false
  modoEdicion.value = false
  certificadoEditando.value = null
  certificadoProcesado.value = false
  errorContratoDuplicado.value = false
  verificandoContrato.value = false
  resetearFormulario()
}

// Función para resetear el formulario
const resetearFormulario = (prellenarFechaHoy = false) => {
  const userInfo = authStore.user || authService.getUserInfo() || {}
  const usuarioNombre = userInfo.usuario || userInfo.nombre || ''
  
  formulario.value = {
    nombreTitular: '',
    apellidosTitular: '',
    numeroContrato: '',
    fechaExpedicion: prellenarFechaHoy ? obtenerFechaHoy() : '',
    aniosVigencia: prellenarFechaHoy ? 1 : null,
    vigenteDesde: '',
    vigenteHasta: '',
    tipoVehiculo: prellenarFechaHoy ? 'NU' : '',
    marca: prellenarFechaHoy ? 'BYD' : '',
    submarca: '',
    modelo: '',
    numeroSerie: '',
    descripcionVehiculo: '',
    version: '',
    usuario: usuarioNombre,
    creadoPor: usuarioNombre,
    estado: 'Solicitado'
  }
  
  // Limpiar estados de validación de contrato
  errorContratoDuplicado.value = false
  verificandoContrato.value = false
  coberturasAgregadas.value = []
}

// Función para validar todos los campos
const validarCampos = () => {
  const camposFaltantes = []
  
  // Validar cada campo usando las refs
  if (!formulario.value.nombreTitular || formulario.value.nombreTitular.trim() === '') {
    camposFaltantes.push('Nombre')
  }
  if (!formulario.value.apellidosTitular || formulario.value.apellidosTitular.trim() === '') {
    camposFaltantes.push('Apellidos')
  }
  const noContrato = (formulario.value.numeroContrato || '').trim()
  if (!noContrato) {
    camposFaltantes.push('Número de contrato')
  } else if (!/^\d{10}$/.test(noContrato)) {
    camposFaltantes.push('Número de contrato (10 dígitos)')
  }
  if (!formulario.value.fechaExpedicion || formulario.value.fechaExpedicion.trim() === '') {
    camposFaltantes.push('Fecha expedición')
  }
  if (formulario.value.aniosVigencia === null || formulario.value.aniosVigencia === undefined || formulario.value.aniosVigencia === '') {
    camposFaltantes.push('Años vigencia')
  }
  if (!formulario.value.tipoVehiculo || formulario.value.tipoVehiculo.trim() === '') {
    camposFaltantes.push('Tipo de vehículo')
  }
  if (!formulario.value.marca || formulario.value.marca.trim() === '') {
    camposFaltantes.push('Marca')
  }
  if (!formulario.value.submarca || formulario.value.submarca.trim() === '') {
    camposFaltantes.push('Submarca')
  }
  if (!formulario.value.modelo || formulario.value.modelo.trim() === '') {
    camposFaltantes.push('Modelo')
  } else if (formulario.value.tipoVehiculo === 'NU') {
    const val = String(formulario.value.modelo || '').trim()
    const y = new Date().getFullYear()
    const ySig = y + 1
    const yAnt = y - 1
    if (!/^\d{4}$/.test(val)) {
      camposFaltantes.push('Modelo (año de 4 dígitos)')
    } else {
      const n = parseInt(val, 10)
      if (!isNaN(n) && n !== ySig && n !== y && n !== yAnt) {
        camposFaltantes.push(`Modelo (solo ${ySig}, ${y} o ${yAnt})`)
      }
    }
  }
  if (!formulario.value.numeroSerie || formulario.value.numeroSerie.trim() === '') {
    camposFaltantes.push('No. de serie')
  }
  
  return camposFaltantes
}

// Función para guardar o actualizar el certificado
const guardarCertificado = async () => {
  if (!coberturasAgregadas.value || coberturasAgregadas.value.length === 0) {
    $q.notify({
      type: 'negative',
      message: 'Debe agregar al menos una cobertura',
      position: 'top',
      timeout: 5000
    })
    return
  }
  // Validar el formulario usando la ref del form
  const valid = await formRef.value?.validate()
  
  if (!valid) {
    // Obtener campos faltantes
    const camposFaltantes = validarCampos()
    
    if (camposFaltantes.length > 0) {
      const mensaje = camposFaltantes.length === 1 
        ? `Falta capturar el campo: ${camposFaltantes.join(', ')}`
        : `Faltan capturar los siguientes campos: ${camposFaltantes.join(', ')}`
      
      $q.notify({
        type: 'negative',
        message: mensaje,
        position: 'top',
        timeout: 5000
      })
    }
    return
  }
  
  // Mostrar diálogo de confirmación según el modo
  const mensajeConfirmacion = modoEdicion.value
    ? '¿Está seguro que desea actualizar este certificado?'
    : '¿Está seguro que desea dar de alta un nuevo certificado?'
  
  $q.dialog({
    title: 'Confirmar',
    message: mensajeConfirmacion,
    cancel: {
      label: 'Cancelar',
      flat: true
    },
    ok: {
      label: 'Aceptar',
      push: true,
      color: 'negative',
      textColor: 'white'
    },
    persistent: true
  }).onOk(async () => {
    // Usuario confirmó, proceder con el guardado o actualización
    if (modoEdicion.value) {
      await actualizarCertificado()
    } else {
      await enviarCertificado()
    }
    await cargarCertificados()
  })
}

// Función para enviar el certificado al API
const enviarCertificado = async () => {
  guardando.value = true
  
  try {
    // Obtener información del usuario autenticado
    const userInfo = authStore.user || authService.getUserInfo() || {}
    const creadoPorEmail = userInfo.email || userInfo.usuario || 'tester.api@dakotamobility.com.mx'
    
    // Obtener la agencia seleccionada
    const agenciaSeleccionada = authStore.agenciaSeleccionada || authService.getAgenciaSeleccionada()
    
    // Formatear fechas al formato ISO requerido (YYYY-MM-DDTHH:mm:ss)
    const formatearFechaISO = (fecha) => {
      if (!fecha) return null
      // Si ya tiene formato ISO, asegurarse de que tenga la hora
      if (fecha.includes('T')) {
        return fecha
      }
      // Si es solo fecha, agregar hora 00:00:00
      return `${fecha}T00:00:00`
    }
    
    const nombreTrim = (formulario.value.nombreTitular || '').trim()
    const apellidosTrim = (formulario.value.apellidosTitular || '').trim()
    const titularConcat = [nombreTrim, apellidosTrim].filter(Boolean).join(' ')

    // Crear el payload según la especificación (crea-certificado / InsertarCertificado)
    const payload = {
      creadoPor: creadoPorEmail,
      titular: titularConcat,
      nombre: nombreTrim,
      apellidos: apellidosTrim,
      // Compatibilidad backend (si el DTO usa PascalCase)
      Nombre: nombreTrim,
      Apellidos: apellidosTrim,
      // Compatibilidad backend (si el DTO usa otros nombres)
      nombreTitular: nombreTrim,
      apellidosTitular: apellidosTrim,
      NombreTitular: nombreTrim,
      ApellidosTitular: apellidosTrim,
      numeroContrato: formulario.value.numeroContrato,
      fechaExpedicion: formatearFechaISO(formulario.value.fechaExpedicion),
      vigenteDesde: formatearFechaISO(formulario.value.vigenteDesde),
      vigenteHasta: formatearFechaISO(formulario.value.vigenteHasta),
      tipoVehiculo: formulario.value.tipoVehiculo,
      marca: formulario.value.marca,
      submarca: formulario.value.submarca,
      modelo: formulario.value.modelo,
      serie: formulario.value.numeroSerie,
      version: formulario.value.version,
      Ampara: formulario.value.Ampara,
      primaNeta: formulario.value.primaNeta,
      primaTotal: formulario.value.primaTotal,
    }
    
    console.log('Payload crea-certificado:', JSON.stringify(payload))
    
    // Agregar agenciaId si hay una agencia seleccionada
    if (agenciaSeleccionada && agenciaSeleccionada.agenciaId) {
      payload.agenciaId = agenciaSeleccionada.agenciaId
    }
    
    // Enviar al API
    const response = await certificadoService.crearCertificado(payload)
    
    // Verificar si fue exitoso (code === 0)
    if (response.code === 0 && response.certificado) {
      // Agregar el certificado al array de rows con Status y polizaStatusId
      const nuevoCertificado = {
        ...response.certificado,
        // Preservar datos aunque el backend no los regrese
        nombre: nombreTrim,
        apellidos: apellidosTrim,
        version: payload.version || '',
        numeroContrato: formulario.value.numeroContrato, // Asegurar que se incluya
        tipoVehiculo: formulario.value.tipoVehiculo, // Asegurar que se incluya
        polizaStatus: 'Solicitado',
        polizaStatusId: 7
      }
      rows.value.unshift(nuevoCertificado)
      
      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: response.message || 'Certificado creado correctamente',
        position: 'top',
        timeout: 3000
      })
      
      // Cerrar el diálogo y resetear el formulario
      cerrarDialog()
    } else {
      // Error en la respuesta
      $q.notify({
        type: 'negative',
        message: response.message || 'Error al crear el certificado',
        position: 'top',
        timeout: 5000
      })
    }
  } catch (error) {
    console.error('Error al guardar certificado:', error)
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.mensaje || 
                        'Error al guardar el certificado. Por favor, intente nuevamente.'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    })
  } finally {
    guardando.value = false
  }
}

// Función para actualizar el certificado
const actualizarCertificado = async () => {
  guardando.value = true
  
  try {
    // Obtener información del usuario autenticado
    const userInfo = authStore.user || authService.getUserInfo() || {}
    const modificadoPorEmail = userInfo.email || userInfo.usuario || 'tester.api@dakotamobility.com.mx'
    
    // Formatear fechas al formato ISO requerido (YYYY-MM-DDTHH:mm:ss)
    const formatearFechaISO = (fecha) => {
      if (!fecha) return null
      // Si ya tiene formato ISO, asegurarse de que tenga la hora
      if (fecha.includes('T')) {
        return fecha
      }
      // Si es solo fecha, agregar hora 00:00:00
      return `${fecha}T00:00:00`
    }
    
    // Obtener el noCertificado del certificado que se está editando
    const certificadoActual = rows.value.find(r => r.uid === certificadoEditando.value)
    const noCertificado = certificadoActual?.noCertificado || ''
    
    const titularMod = construirTitularDesdeForm(formulario.value)
    const nombreTrim = (formulario.value.nombreTitular || '').trim()
    const apellidosTrim = (formulario.value.apellidosTitular || '').trim()

    // Crear el payload para actualización
    const payload = {
      uid: certificadoEditando.value,
      noCertificado: noCertificado,
      modificadoPor: modificadoPorEmail,
      titular: titularMod,
      nombre: nombreTrim,
      apellidos: apellidosTrim,
      // Compatibilidad backend (si el DTO usa PascalCase)
      Nombre: nombreTrim,
      Apellidos: apellidosTrim,
      // Compatibilidad backend (si el DTO usa otros nombres)
      nombreTitular: nombreTrim,
      apellidosTitular: apellidosTrim,
      NombreTitular: nombreTrim,
      ApellidosTitular: apellidosTrim,
      numeroContrato: formulario.value.numeroContrato,
      fechaExpedicion: formatearFechaISO(formulario.value.fechaExpedicion),
      vigenteDesde: formatearFechaISO(formulario.value.vigenteDesde),
      vigenteHasta: formatearFechaISO(formulario.value.vigenteHasta),
      tipoVehiculo: formulario.value.tipoVehiculo,
      marca: formulario.value.marca,
      submarca: formulario.value.submarca,
      modelo: formulario.value.modelo,
      serie: formulario.value.numeroSerie,
      version: formulario.value.tipoVehiculo === 'SE' ? (formulario.value.version || '') : '',
      Ampara: formulario.value.Ampara,
      primaNeta: formulario.value.primaNeta,
      primaTotal: formulario.value.primaTotal,
    }
    
    console.log('Payload modificar-certificado:', JSON.stringify(payload))
    // Enviar al API para actualizar
    const response = await certificadoService.actualizarCertificado(payload)
    
    // Verificar si fue exitoso (message === "success")
    if (response.message === 'success') {
      // Actualizar el certificado en el array de rows con los datos modificados
      const index = rows.value.findIndex(r => r.uid === certificadoEditando.value)
      if (index !== -1) {
        // Actualizar el row con los datos del formulario (ya formateados)
        rows.value[index] = {
          ...rows.value[index], // Mantener datos existentes
          titular: titularMod,
          nombre: nombreTrim,
          apellidos: apellidosTrim,
          numeroContrato: formulario.value.numeroContrato, // Incluir número de contrato
          fechaExpedicion: formatearFechaISO(formulario.value.fechaExpedicion),
          vigenteDesde: formatearFechaISO(formulario.value.vigenteDesde),
          vigenteHasta: formatearFechaISO(formulario.value.vigenteHasta),
          tipoVehiculo: formulario.value.tipoVehiculo, // Incluir tipo de vehículo
          marca: formulario.value.marca,
          submarca: formulario.value.submarca,
          modelo: formulario.value.modelo,
          serie: formulario.value.numeroSerie,
          ampara: formulario.value.Ampara,
          primaNeta: formulario.value.primaNeta,
          primaTotal: formulario.value.primaTotal,
          // Si la respuesta incluye un certificado actualizado, usar esos datos
          ...(response.certificado || {}),
          // Preservar versión si el backend la devuelve vacía/no la devuelve
          version: (response.certificado?.version ?? response.certificado?.Version) || payload.version || ''
        }
      }
      
      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: 'Certificado actualizado correctamente',
        position: 'top',
        timeout: 3000
      })
      
      // Cerrar el diálogo y resetear el formulario
      cerrarDialog()
    } else {
      // Error en la respuesta
      $q.notify({
        type: 'negative',
        message: response.message || 'Error al actualizar el certificado',
        position: 'top',
        timeout: 5000
      })
    }
  } catch (error) {
    console.error('Error al actualizar certificado:', error)
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.mensaje || 
                        'Error al actualizar el certificado. Por favor, intente nuevamente.'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    })
  } finally {
    guardando.value = false
  }
}

// Función para descargar certificado
const descargarCertificado = async (row) => {
  // Mostrar diálogo de confirmación
  $q.dialog({
    title: 'Generar Certificado PDF',
    message: `¿Está seguro que desea generar el certificado PDF para ${row.noCertificado}?`,
    cancel: true,
    persistent: true,
    ok: {
      label: 'Generar',
      color: 'negative',
      textColor: 'white',
      push: true
    },
    cancel: {
      label: 'Cancelar',
      flat: true
    }
  }).onOk(async () => {
    // Usuario confirmó, proceder con la generación
    try {
      // Mostrar notificación de carga
      $q.notify({
        type: 'info',
        message: `Generando certificado ${row.noCertificado}...`,
        position: 'top',
        timeout: 2000
      })

      // Llamar al servicio para generar el PDF
      const pdfBlob = await certificadoService.generarPdf(row.noCertificado)

      // Construir el nombre del archivo: {NoCertificado}_{Titular}.pdf
      const titularLimpio = (row.titular || '').replace(/[^a-zA-Z0-9]/g, '_').trim()
      const filename = `${row.noCertificado}_${titularLimpio}.pdf`

      // Crear URL del blob
      const url = window.URL.createObjectURL(pdfBlob)
      
      // Abrir el PDF en una nueva pestaña
      // Nota: Los blob URLs siempre muestran un GUID en la barra de direcciones
      // pero el PDF se abrirá correctamente y el navegador puede mostrar
      // el nombre del archivo en el título de la pestaña
      const newWindow = window.open(url, '_blank')
      
      // Si el navegador bloquea la ventana emergente, descargar el archivo
      if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
        // Fallback: descargar el archivo con el nombre correcto
        const link = document.createElement('a')
        link.href = url
        link.download = filename
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        
        $q.notify({
          type: 'info',
          message: 'El PDF se descargará automáticamente. Por favor, permita ventanas emergentes para abrir en nueva pestaña.',
          position: 'top',
          timeout: 4000
        })
      }
      
      // Limpiar la URL después de un tiempo (la ventana ya la tiene cargada)
      setTimeout(() => {
        window.URL.revokeObjectURL(url)
      }, 2000)

      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: `Certificado ${row.noCertificado} generado correctamente`,
        position: 'top',
        timeout: 3000
      })
    } catch (error) {
      console.error('Error al generar certificado:', error)
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.mensaje || 
                          'Error al generar el certificado. Por favor, intente nuevamente.'
      
      $q.notify({
        type: 'negative',
        message: errorMessage,
        position: 'top',
        timeout: 5000
      })
    }
  })
}

// Función para cargar certificados desde la API
const cargarCertificados = async () => {
  loading.value = true
  
  try {
    // Obtener la agencia seleccionada
    const agenciaSeleccionada = authStore.agenciaSeleccionada || authService.getAgenciaSeleccionada()
    
    // Preparar el payload con las fechas y agenciaId
    const payload = {
      fechaExpedicionDesde: filtroFechaDesde.value || '',
      fechaExpedicionHasta: filtroFechaHasta.value || ''
    }
    
    // Agregar agenciaId si hay una agencia seleccionada
    if (agenciaSeleccionada && agenciaSeleccionada.agenciaId) {
      payload.agenciaId = agenciaSeleccionada.agenciaId
    }
    
    // Llamar al servicio
    const response = await certificadoService.obtenerCertificados(payload)
    
    // Verificar si fue exitoso (code === 0)
    if (response.code === 0 && response.certificados) {
      rows.value = response.certificados
    } else if (response.code === 0 && Array.isArray(response.data)) {
      // Si la respuesta viene en data en lugar de certificados
      rows.value = response.data
    } else {
      // Si no hay certificados, inicializar array vacío
      rows.value = []
      
      if (response.message) {
        $q.notify({
          type: 'info',
          message: response.message,
          position: 'top',
          timeout: 3000
        })
      }
    }
  } catch (error) {
    console.error('Error al cargar certificados:', error)
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.mensaje || 
                        'Error al cargar los certificados. Por favor, intente nuevamente.'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    })
    
    // En caso de error, inicializar array vacío
    rows.value = []
  } finally {
    loading.value = false
  }
}

// Función para buscar certificados (se ejecuta al hacer clic en el botón Buscar)
const buscarCertificados = () => {
  // Validar que ambas fechas estén capturadas
  if (!filtroFechaDesde.value || !filtroFechaHasta.value) {
    $q.notify({
      type: 'negative',
      message: 'Debe capturar ambas fechas (Desde y Hasta) para realizar la búsqueda',
      position: 'top',
      timeout: 5000
    })
    return
  }
  
  // Validar que la fecha "Desde" no sea mayor que la fecha "Hasta"
  if (new Date(filtroFechaDesde.value) > new Date(filtroFechaHasta.value)) {
    $q.notify({
      type: 'negative',
      message: 'La fecha "Desde" no puede ser mayor que la fecha "Hasta"',
      position: 'top',
      timeout: 5000
    })
    return
  }
  
  cargarCertificados()
}

// Función para obtener el primer día del mes actual en formato YYYY-MM-DD
const obtenerPrimerDiaDelMes = () => {
  const hoy = new Date()
  const primerDia = new Date(hoy.getFullYear(), hoy.getMonth(), 1)
  return primerDia.toISOString().split('T')[0]
}

// Función para obtener la fecha de hoy en formato YYYY-MM-DD
const obtenerFechaHoy = () => {
  const hoy = new Date()
  return hoy.toISOString().split('T')[0]
}

// Computed para obtener agencias del store
const agencias = computed(() => {
  return authStore.agencias || []
})

// Filtrar agencias según el texto de búsqueda
const agenciasFiltradas = computed(() => {
  if (!filtroAgencia.value || filtroAgencia.value.trim() === '') {
    return agencias.value
  }
  
  const filtro = filtroAgencia.value.toLowerCase().trim()
  
  return agencias.value.filter(agencia => {
    const nombreAgencia = (agencia.agencia || '').toLowerCase()
    const id = (agencia.bbvaAgenciaId || '').toString().toLowerCase()
    const estado = (agencia.entidadFederativa || '').toLowerCase()
    const division = (agencia.division || '').toLowerCase()
    
    return nombreAgencia.includes(filtro) ||
           id.includes(filtro) ||
           estado.includes(filtro) ||
           division.includes(filtro)
  })
})

// Función para seleccionar una agencia
const seleccionarAgencia = async (agencia) => {
  authStore.seleccionarAgencia(agencia)
  filtroAgencia.value = '' // Limpiar filtro al seleccionar
  dialogAgenciaOpen.value = false
  
  $q.notify({
    type: 'positive',
    message: `Agencia ${agencia.agencia || ''} seleccionada`,
    position: 'top',
    timeout: 2000
  })
  
  // Cargar certificados después de seleccionar la agencia
  await cargarCertificados()
}

// Watcher para detectar cambios en la agencia seleccionada y recargar certificados
watch(() => authStore.agenciaSeleccionada, async (nuevaAgencia, agenciaAnterior) => {
  // Solo recargar si hay una nueva agencia seleccionada y es diferente a la anterior
  if (nuevaAgencia && nuevaAgencia !== agenciaAnterior) {
    await cargarCertificados()
  }
}, { deep: true })

// Verificar autenticación al cargar y asignar fechas por defecto
onMounted(async () => {
  authStore.checkAuth()
  if (!authStore.isAuthenticated) {
    router.push('/login').catch(() => {
      window.location.href = '/#/login'
    })
    return
  }
  
  // Verificar si hay agencias disponibles y no hay una seleccionada
  const agenciasDisponibles = authStore.agencias && authStore.agencias.length > 0
  const agenciaSeleccionada = authStore.agenciaSeleccionada
  
  if (agenciasDisponibles && !agenciaSeleccionada) {
    // Limpiar filtro y mostrar el modal de selección de agencia
    filtroAgencia.value = ''
    mostrarBotonCerrar.value = false // No mostrar botón cuando se abre automáticamente después del login
    dialogAgenciaOpen.value = true
  }
  
  // Asignar fechas por defecto: primer día del mes actual y fecha de hoy
  filtroFechaDesde.value = obtenerPrimerDiaDelMes()
  filtroFechaHasta.value = obtenerFechaHoy()
  
  // Cargar certificados solo si hay una agencia seleccionada
  const agenciaSeleccionadaActual = authStore.agenciaSeleccionada || authService.getAgenciaSeleccionada()
  if (agenciaSeleccionadaActual) {
    await cargarCertificados()
  }
})
</script>

<style scoped>
.dashboard-page {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 64px); /* Altura total menos el header */
  padding: 16px;
}

.dashboard-table {
  width: 100%;
  height: 100%;
  flex: 1;
  min-height: 0;
}

/* Asegurar que la tabla interna sea responsive y ocupe todo el espacio */
.dashboard-table :deep(.q-table__container) {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.dashboard-table :deep(.q-table__top) {
  flex-shrink: 0;
}

.dashboard-table :deep(.q-table__middle) {
  flex: 1;
  overflow: auto;
  min-height: 0;
}

.dashboard-table :deep(.q-table__bottom) {
  flex-shrink: 0;
}

/* Estilos para el hipervínculo del titular */
.titular-link {
  color: orangered;
  text-decoration: none;
  cursor: pointer;
  font-weight: 500;
}

.titular-link:hover {
  text-decoration: underline;
  color: #ff4500;
}

.titular-link:active {
  color: #cc3700;
}

/* Estilos del diálogo Nuevo certificado */
.certificado-dialog :deep(.q-field--focused .q-field__label),
.certificado-dialog :deep(.q-field--highlighted .q-field__label) {
  color: #757575 !important;
}

.certificado-dialog :deep(.q-field--focused .q-field__control:before),
.certificado-dialog :deep(.q-field--focused .q-field__control:after) {
  border-color: #757575 !important;
}

.certificado-dialog :deep(.q-field--focused .q-field__marginal),
.certificado-dialog :deep(.q-field--focused .q-icon) {
  color: #757575 !important;
}
.certificado-dialog__actions {
  flex-shrink: 0;
  border-top: 1px solid #e0e0e0;
}
.certificado-dialog {
  max-height: 90vh;
}

.certificado-dialog__body {
  min-height: 0;
  flex: 1;
}

.certificado-dialog__main {
  min-height: 0;
}

.certificado-dialog__header {
  background: #1a1a1a;
  color: #fff;
  flex-shrink: 0;
  padding: 12px 16px;
  height: auto;
}

.certificado-dialog__header-sub {
  color: #9e9e9e;
}

.ticket-btn--add-cobertura {
  background: #1fef37a8 !important;
  color: #ffffff !important;
  padding: 2px 10px !important;
  font-size: 8px !important;
    margin-left: auto;
  display: block;
}

/* Estilos para el chip de Status */
.status-chip {
  font-size: 11px;
  letter-spacing: 0.06em;
}

/* Responsive: ajustar en pantallas pequeñas */
@media (max-width: 600px) {
  .dashboard-page {
    padding: 8px;
  }
}
/* Menú teletransportado por q-btn-dropdown — sin scoped */
.ticket-cobertura-dropdown {
  min-width: 340px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #90caf9;
  box-shadow: 0 10px 28px rgba(13, 71, 161, 0.18);
}

.ticket-cobertura-dropdown__list {
  padding: 0;
}

.ticket-cobertura-dropdown__header {
  background: linear-gradient(135deg, #0d47a1 0%, #1565c0 55%, #1976d2 100%);
  color: #fff;
  font-weight: 600;
  font-size: 0.82rem;
  letter-spacing: 0.02em;
  padding: 10px 14px;
  margin: 0;
}

.ticket-cobertura-dropdown__item {
  padding: 10px 12px;
  border-left: 3px solid transparent;
  transition: background 0.15s ease, border-color 0.15s ease;
}

.ticket-cobertura-dropdown__item:hover {
  background: linear-gradient(90deg, #e3f2fd 0%, #e8f4fd 100%);
  border-left-color: #1565c0;
}

.ticket-cobertura-dropdown__item + .ticket-cobertura-dropdown__item {
  border-top: 1px solid #e3f2fd;
}



.ticket-cobertura-dropdown__label {
  font-weight: 600;
  color: #0d47a1;
  line-height: 1.35;
  white-space: normal;
}

.ticket-cobertura-dropdown__caption {
  color: #1976d2;
  font-size: 0.72rem;
  margin-top: 2px;
}

.ticket-cobertura-dropdown__add-icon {
  color: #1565c0;
  opacity: 0.55;
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.ticket-cobertura-dropdown__item:hover .ticket-cobertura-dropdown__add-icon {
  opacity: 1;
  transform: scale(1.08);
}

.ticket-cobertura-dropdown__empty {
  padding: 12px;
  background: #fafafa;
}
.ticket-cobertura-qtable :deep(thead tr th) {
  background: #040708f9 !important;
  color: #eceff1 !important;
  font-weight: 600;
}

.ticket-cobertura-qtable :deep(thead tr th) {
  background: #1a1a1a !important;
  color: #ffffff !important;
}

.ticket-cobertura-qtable :deep(tbody tr:nth-child(even)) {
  background-color: #f5f5f5;
}

</style>
